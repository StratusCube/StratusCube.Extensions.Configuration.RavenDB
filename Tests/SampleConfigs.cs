﻿using System;

namespace Tests {
    public class HttpClientConfig {
        public Uri? BaseAddress { get; set; }
        public TimeSpan Timeout { get; set; }
    }

    public class HttpClientConfigs {
        public HttpClientConfig GitHubApi { get; set; } = new();
        public HttpClientConfig TimeApi { get; set; } = new();
    }
}
