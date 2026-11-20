import type {
  FetchLike,
  ProviderHttpRequestInit,
  ProviderHttpResponse,
} from '../providers/provider-boundary.js';

type CrucibleFetch = FetchLike;
type CrucibleFetchInit = ProviderHttpRequestInit;
type CrucibleFetchResponse = ProviderHttpResponse;

export type { CrucibleFetch, CrucibleFetchInit, CrucibleFetchResponse };
