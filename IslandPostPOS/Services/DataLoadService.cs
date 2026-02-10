using CommunityToolkit.Mvvm.ComponentModel;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IslandPostPOS.Services;

public abstract partial class DataLoaderService : ObservableObject
{
    private readonly HttpClient _httpClient;
    private bool _isInitialized;
    private static readonly IAsyncPolicy<HttpResponseMessage> _httpPolicy =
    Policy.WrapAsync(
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(100)), // generic timeout
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            )
    );
    protected DataLoaderService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    // Non-generic interface
    public interface ILoaderDescriptor
    {
        string Name { get; }
        Task<string> LoadAsync(HttpClient client, CancellationToken ct);
    }

    // Generic implementation
    public record LoaderDescriptor<TDto>(
        string Endpoint,
        string Name,
        Action<ObservableCollection<TDto>> Assign
    ) : ILoaderDescriptor
    {
        public async Task<string> LoadAsync(HttpClient client, CancellationToken ct)
        {
            try
            {
                var response = await _httpPolicy.ExecuteAsync(
                    async token =>
                    {
                        var resp = await client.GetAsync(Endpoint, token);
                        resp.EnsureSuccessStatusCode();
                        return resp;
                    },
                    ct);

                var dtos = await response.Content.ReadFromJsonAsync<List<TDto>>(cancellationToken: ct);
                Assign(new ObservableCollection<TDto>(dtos ?? new List<TDto>()));

                return Name;
            }
            catch (TimeoutRejectedException)
            {
                throw new TimeoutException($"Request to {Endpoint} timed out.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load {Name} from {Endpoint}.", ex);
            }
        }
    }

    /// <summary>
    /// Generic loader for any DTO collection.
    /// </summary>
    protected async Task<string> LoadCollectionAsync<TDto>(
        LoaderDescriptor<TDto> descriptor,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(descriptor.Endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        var dtos = await response.Content.ReadFromJsonAsync<List<TDto>>(cancellationToken: cancellationToken);
        descriptor.Assign(new ObservableCollection<TDto>(dtos ?? new List<TDto>()));

        return descriptor.Name;
    }

    /// <summary>
    /// Initialize service by running all descriptors and reporting progress.
    /// </summary>
    protected async Task InitializeAsync(
        IEnumerable<ILoaderDescriptor> descriptors,
        Action<string, double>? reportProgress = null,
        CancellationToken cancellationToken = default)
    {
        if (_isInitialized) return;

        int total = descriptors.Count();
        int completed = 0;

        var runningTasks = descriptors.Select(async d =>
        {
            // Report starting
            reportProgress?.Invoke($"Loading {d.Name}...", (double)completed / total * 100.0);

            try
            {
                var name = await d.LoadAsync(_httpClient, cancellationToken);

                // Report finished
                int done = Interlocked.Increment(ref completed);
                double percent = (double)done / total * 100.0;
                reportProgress?.Invoke($"Loaded {name}.", percent);
            }
            catch (Exception ex)
            {
                int done = Interlocked.Increment(ref completed);
                reportProgress?.Invoke($"Error loading {d.Name}: {ex.Message}", (double)done / total * 100.0);
            }
        });

        await Task.WhenAll(runningTasks);
        _isInitialized = true;
    }
}