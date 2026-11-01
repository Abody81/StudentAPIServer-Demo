// Activates the browser tools client in Blazor apps that render on the server (static SSR or
// Interactive Server), which the browser refresh TagHelper does not reach.
//
// The provider reports its availability over a stable application route. The configuration module
// is app hosted, so resolving it relative to import.meta.url keeps it correct under any static web
// asset base path or fingerprinting scheme.

// Substituted by Microsoft.NET.Sdk.StaticWebAssets.DotNetWatch.targets.
const settingsPath = '/_framework/dotnet-browser-tools/hot-reload-settings.json';
const configModulePath = './Microsoft.NET.Sdk.BlazorWeb.DotNetWatch.BrowserTools.Config.js';
const settingsRequestTimeoutMilliseconds = 5000;

export async function afterWebStarted() {
    if (!await isHotReloadEnabled()) {
        return;
    }

    // The browser tools client de-duplicates activation, so importing the configuration module is safe
    // even when another initializer or the browser refresh TagHelper already imported it.
    await import(new URL(configModulePath, import.meta.url).href);
}

async function isHotReloadEnabled() {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), settingsRequestTimeoutMilliseconds);

    try {
        const response = await fetch(settingsPath, { cache: 'no-store', signal: controller.signal });
        if (!response.ok) {
            return false;
        }

        const settings = await response.json();
        return settings?.hotReload === true;
    } catch (error) {
        console.debug('Unable to read the dotnet-watch browser tools settings.', error);
        return false;
    } finally {
        clearTimeout(timeout);
    }
}

// SIG // Begin signature block
// SIG // MIInMwYJKoZIhvcNAQcCoIInJDCCJyACAQExDzANBglg
// SIG // hkgBZQMEAgEFADB3BgorBgEEAYI3AgEEoGkwZzAyBgor
// SIG // BgEEAYI3AgEeMCQCAQEEEBDgyQbOONQRoqMAEEvTUJAC
// SIG // AQACAQACAQACAQACAQAwMTANBglghkgBZQMEAgEFAAQg
// SIG // /qhu7Z8GwwscZ/L0apgXiiDrNtAGWjKcnWY4lvYOvoWg
// SIG // ggypMIIF5DCCA8ygAwIBAgITMwAAAcgkOsgeTZOIuwAA
// SIG // AAAByDANBgkqhkiG9w0BAQsFADBXMQswCQYDVQQGEwJV
// SIG // UzEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9u
// SIG // MSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBTaWduaW5n
// SIG // IFBDQSAyMDI0MB4XDTI2MDQxNjE4NTc0MVoXDTI3MDQx
// SIG // NTE4NTc0MVowYzELMAkGA1UEBhMCVVMxEzARBgNVBAgT
// SIG // Cldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAc
// SIG // BgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjENMAsG
// SIG // A1UEAxMELk5FVDCCASIwDQYJKoZIhvcNAQEBBQADggEP
// SIG // ADCCAQoCggEBAMJfsAP8OxCUF6ivbFnD75BCsACvUPfi
// SIG // Af/LQ2AsqaCvJWtki//fPx4BhRWbvVuA5Tn1RvESIwAA
// SIG // oBJic0fJfjHYrZeQ4Ga7AxdGESwJMP5LZYfbVYKef3k/
// SIG // slhO8ygk5JNvxgos4lfXPCoBDFNm9a0fHfrRjgEVOmLb
// SIG // andQPFM0MSKk+kiJtSIpB3dIWQhaMQ3t0XwVHo3XEDYo
// SIG // SY3qU/npr6shcxBFgdZcTf0AQsITzCHPFDUS+iPqdmLh
// SIG // /PIokqOhmxIgd5Q69vqYjASwq1sg9peqfsJKVY4C0idy
// SIG // /Uf4AtWU6qmfxbQGrxphgBM4YU21HyQ1CgtEscHdSEKk
// SIG // TrcCAwEAAaOCAZswggGXMA4GA1UdDwEB/wQEAwIHgDAf
// SIG // BgNVHSUEGDAWBgorBgEEAYI3TAgBBggrBgEFBQcDAzAd
// SIG // BgNVHQ4EFgQUwduFP95X0NdybDKqpv9jV3T9cfIwRQYD
// SIG // VR0RBD4wPKQ6MDgxHjAcBgNVBAsTFU1pY3Jvc29mdCBD
// SIG // b3Jwb3JhdGlvbjEWMBQGA1UEBRMNNDY0MjIzKzUwNzU5
// SIG // NjAfBgNVHSMEGDAWgBR/WT9UIdqtT+8F5eaj1y0GlBII
// SIG // MTBgBgNVHR8EWTBXMFWgU6BRhk9odHRwOi8vd3d3Lm1p
// SIG // Y3Jvc29mdC5jb20vcGtpb3BzL2NybC9NaWNyb3NvZnQl
// SIG // MjBDb2RlJTIwU2lnbmluZyUyMFBDQSUyMDIwMjQuY3Js
// SIG // MG0GCCsGAQUFBwEBBGEwXzBdBggrBgEFBQcwAoZRaHR0
// SIG // cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9jZXJ0
// SIG // cy9NaWNyb3NvZnQlMjBDb2RlJTIwU2lnbmluZyUyMFBD
// SIG // QSUyMDIwMjQuY3J0MAwGA1UdEwEB/wQCMAAwDQYJKoZI
// SIG // hvcNAQELBQADggIBAKNTJ0R6r3T/SAhdOUibRuJvosAa
// SIG // IssCd+bb8aUe/hDVbBovDkE3EtgWAm5CPeFOeYiqmpCt
// SIG // 3aJntyMvA/qtLuWeJymwvCd4E/qo2If/trHxzZ12yGjo
// SIG // xp822b46DKIFFbg0mvZ74f0WVRu+Ftae93BT4Hid2x0h
// SIG // MYQWtIgTjoGH0FJCETvfTBBgegscj1AY+zghBpgeK2Df
// SIG // 5+zZdCGr6s/s6/xecllnuoJIdD35AW4G/5yqjU7oztCZ
// SIG // Wzr21L1Dlsq6RYuFg/2yym7hd9V+Au6XojrKbpR9Vi5v
// SIG // HqqWXjmE6QF/BXRSXzNHmqZhtCjKynbcMHNeQkqy+5/3
// SIG // iPSvAs8Fd6oVHRJxc+CFCWVirnpm9tQHNvfyvEsjTxjm
// SIG // e5pyWvnz6bGQM8bm30Zto/6F1zviqMTCJ2+OORn3wFZv
// SIG // F+63NT4UB9FH7SxjdgFZoprwvQxnzvCujaOfosARxt/7
// SIG // DrOF26bvdUNRsJQSjhdzGVZp5nLCT6aYONqVI75oljrJ
// SIG // kBNTJ2nmNYeUxUKV2iyntZqIUIqHd+SHDAn+GcoI+Jo1
// SIG // 1J3pOuCTl6OygDdPJZW5UfcglKCfdoHSzFAEH/dvpT+U
// SIG // 8sA3xIseAAsH3nUjOwadeMLu1jkRC40u7CcVfqaDuUO9
// SIG // 58az54AQfjwWRT3JxzVAJT1v0BX6gkkyemgCcawdMIIG
// SIG // vTCCBKWgAwIBAgITMwAAADk7tjcZvwYdZwAAAAAAOTAN
// SIG // BgkqhkiG9w0BAQwFADCBiDELMAkGA1UEBhMCVVMxEzAR
// SIG // BgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1JlZG1v
// SIG // bmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlv
// SIG // bjEyMDAGA1UEAxMpTWljcm9zb2Z0IFJvb3QgQ2VydGlm
// SIG // aWNhdGUgQXV0aG9yaXR5IDIwMTEwHhcNMjQwODA4MjA1
// SIG // NDE4WhcNMzYwMzIyMjIxMzA0WjBXMQswCQYDVQQGEwJV
// SIG // UzEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBvcmF0aW9u
// SIG // MSgwJgYDVQQDEx9NaWNyb3NvZnQgQ29kZSBTaWduaW5n
// SIG // IFBDQSAyMDI0MIICIjANBgkqhkiG9w0BAQEFAAOCAg8A
// SIG // MIICCgKCAgEA2AGcHuM4x6qVFgc1rbrm/ghj18fxMqF6
// SIG // Yd88g17WCktpXd2GDfbhbAfTotwdumewG6QIM2K2vKjR
// SIG // 21L8Rboj/IQv9stJjyEqlt9C5a6wE+v2prNhwPEJb+qe
// SIG // NvkmwjWzxs06DdtUBO9BUvi1x/TdpPJyzPxB4J0zSX/I
// SIG // dE5sn1CprMzWvpU8Q4nssv64QRvvfDpAK6Gzz1rW6/XN
// SIG // 6s5gyeyd5FHAJunJbXUhyCBTRxEoMOrWmNUnMhsgXr6i
// SIG // JddtF46yB4jzO7UXJB8rR9WRrJrxKZUdD+05/beZnhb2
// SIG // TRGLbZBb2ndSBILk5QOS0rHFwKYYvq1ct84ZJYcghXhi
// SIG // tMlNPo823LlESiMcm5kcCuQX1WcdMRFahOMDN8jeQ/7l
// SIG // vhqVR2GABnu2067VtWdd8dqo9iXas+zbSOLTcs5VayH+
// SIG // tp2ATXt6zmEv63qVXR5UetWGyyxE5Ym7PYxcwK3uLDuU
// SIG // U8b0tcoaOyaiefaCi0Z0ci4SDkmckwlWaLF3ktGWSaBh
// SIG // FkrOHFILDKgYirQ+FoDtj5U9y3mkIeSKNwggObSeWQr7
// SIG // QrJ6miVyoabRP8ZhBEyEcmUY46ZVCinfrBRVRiSVTL76
// SIG // 8NZ4SASjizuHE3qYht/YxIhD+Ih8xmAnELr2i6QxRcKs
// SIG // 4LdKQT/EiSCl+XbYwzWK2Rnfmc1eQyiVTWUCAwEAAaOC
// SIG // AU4wggFKMA4GA1UdDwEB/wQEAwIBhjAQBgkrBgEEAYI3
// SIG // FQEEAwIBADAdBgNVHQ4EFgQUf1k/VCHarU/vBeXmo9ct
// SIG // BpQSCDEwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBDAEEw
// SIG // DwYDVR0TAQH/BAUwAwEB/zAfBgNVHSMEGDAWgBRyLToC
// SIG // MZBDuRQFTuHqp8cx0SOJNDBaBgNVHR8EUzBRME+gTaBL
// SIG // hklodHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtpL2Ny
// SIG // bC9wcm9kdWN0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFf
// SIG // MDNfMjIuY3JsMF4GCCsGAQUFBwEBBFIwUDBOBggrBgEF
// SIG // BQcwAoZCaHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3Br
// SIG // aS9jZXJ0cy9NaWNSb29DZXJBdXQyMDExXzIwMTFfMDNf
// SIG // MjIuY3J0MA0GCSqGSIb3DQEBDAUAA4ICAQAUlB84KE/u
// SIG // iefp8sgwqtKU3VZgrAMWAB13KY5Q7cWszx3sH9b+JDoP
// SIG // FewOfsPlbjAzBh4vKy1wSp+SPPg1RFGBrPIy7nJHNCHg
// SIG // uqMDi1K1NwmHWikTGjuefk+48Fidu7T5MdK5UdN7RVNM
// SIG // 9WGKXL+mIWsOjdrFD0/gL46XnJ637aBN96QgJLnFL5xh
// SIG // 9Ii+CfQmSxUFUxhUjlAW7+qGcuGwQURTMbx++/SGOCQ7
// SIG // 6WSlX23LoaQ3i92d3vJrpDppH3LfhqIzWqbFrEGLo5Sf
// SIG // I2Xp+S66f92JMWdgMtOmk6Sv+aDlZJ8KINUw0LG2PjA8
// SIG // oLk6YebUNAi38w2iRtsfdQawU/VBvOwuhy5KosK8fT0i
// SIG // jd8M9OaxxH1jvkbipftFNfwB0E+jQjo4SiN/f3O4Vm3S
// SIG // o4ebrlhZATr1xkza54TUwHTl002Acr2BMTvMq8r9+DwH
// SIG // aqNbzwxP9YXlXm69ka2pr0VIvZFrMCsD6sM+5/okZjPg
// SIG // emAxkcHhLqzNZIpgG/RWKwLN/GB5T52q5db1t3Rq5iU4
// SIG // HnwM9w5gp1zdJ73iD7EvilwSFsHngk6ACTBhO7/10t4f
// SIG // akOp4lkAAFUNZFAJpd87kuDIrAoIthemKCtlgKNRIFyv
// SIG // 5V7w8VYyFVNCXS/irwn8BSZA3lbifXTVxjYvgDsZNAbW
// SIG // HfYccC99ARJY/TGCGeIwghneAgEBMG4wVzELMAkGA1UE
// SIG // BhMCVVMxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jwb3Jh
// SIG // dGlvbjEoMCYGA1UEAxMfTWljcm9zb2Z0IENvZGUgU2ln
// SIG // bmluZyBQQ0EgMjAyNAITMwAAAcgkOsgeTZOIuwAAAAAB
// SIG // yDANBglghkgBZQMEAgEFAKCBrjAZBgkqhkiG9w0BCQMx
// SIG // DAYKKwYBBAGCNwIBBDAcBgorBgEEAYI3AgELMQ4wDAYK
// SIG // KwYBBAGCNwIBFTAvBgkqhkiG9w0BCQQxIgQgbVJga8kC
// SIG // VqBYsocmSlC+OJlwhR6ajUCqS54jU0p92QkwQgYKKwYB
// SIG // BAGCNwIBDDE0MDKgFIASAE0AaQBjAHIAbwBzAG8AZgB0
// SIG // oRqAGGh0dHA6Ly93d3cubWljcm9zb2Z0LmNvbTANBgkq
// SIG // hkiG9w0BAQEFAASCAQC0IkZqVT3//C6CXeox2gXEjQs0
// SIG // ClAzBAgC3lRTFXFuZg9IGolFsS+RK7ysbE9fkSXfKwpK
// SIG // SDZEJpBaEJy3RhDsOd+epXhotTSFopeubwd/v++qAZM6
// SIG // DgYu/HgsY5e3efM+zKx0FbdDyiEmSBSMdgEsUkGkhI+l
// SIG // qZ5Bvc52aZSojowwHIkOWIQKUcjjtVFvmtINAqnl0JJu
// SIG // 8rKzcuP/Qhxdk7gS7xddsQXlcnSkbCVu/yKcUKDs1viP
// SIG // Ic/bQKRli+sULzZnQpsEwn3HuKDc1/q6YXZ10+Kw+CTj
// SIG // PkAdAIcG7qj7rNA+D43YjnmG92wlpw9GKasPVLmuZbuh
// SIG // ZbOJ1OStoYIXlDCCF5AGCisGAQQBgjcDAwExgheAMIIX
// SIG // fAYJKoZIhvcNAQcCoIIXbTCCF2kCAQMxDzANBglghkgB
// SIG // ZQMEAgEFADCCAVIGCyqGSIb3DQEJEAEEoIIBQQSCAT0w
// SIG // ggE5AgEBBgorBgEEAYRZCgMBMDEwDQYJYIZIAWUDBAIB
// SIG // BQAEIBlKi9r3URhpNjHaUNli5oObjH1qNWSJqHGDgpfv
// SIG // u2pjAgZqqkvoCFsYEzIwMjYxMDA0MjI1NTI3LjAxNVow
// SIG // BIACAfSggdGkgc4wgcsxCzAJBgNVBAYTAlVTMRMwEQYD
// SIG // VQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25k
// SIG // MR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24x
// SIG // JTAjBgNVBAsTHE1pY3Jvc29mdCBBbWVyaWNhIE9wZXJh
// SIG // dGlvbnMxJzAlBgNVBAsTHm5TaGllbGQgVFNTIEVTTjoz
// SIG // NzAzLTA1RTAtRDk0NzElMCMGA1UEAxMcTWljcm9zb2Z0
// SIG // IFRpbWUtU3RhbXAgU2VydmljZaCCEeowggcgMIIFCKAD
// SIG // AgECAhMzAAACHzpwaeSiMC6VAAEAAAIfMA0GCSqGSIb3
// SIG // DQEBCwUAMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpX
// SIG // YXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYD
// SIG // VQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAkBgNV
// SIG // BAMTHU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAyMDEw
// SIG // MB4XDTI2MDIxOTE5Mzk1MVoXDTI3MDUxNzE5Mzk1MVow
// SIG // gcsxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5n
// SIG // dG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVN
// SIG // aWNyb3NvZnQgQ29ycG9yYXRpb24xJTAjBgNVBAsTHE1p
// SIG // Y3Jvc29mdCBBbWVyaWNhIE9wZXJhdGlvbnMxJzAlBgNV
// SIG // BAsTHm5TaGllbGQgVFNTIEVTTjozNzAzLTA1RTAtRDk0
// SIG // NzElMCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAg
// SIG // U2VydmljZTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCC
// SIG // AgoCggIBAMs7xcU5x8aoCqCLPT4CZCYWXd1nRpMbhQUm
// SIG // Sqo10wfLbwNqF4IGzo425+6TJ7nHjYJaTOBEjL2QTZVb
// SIG // ASe1nxmSDKvKQLRsiqwgPv7oXGC6x5Nd/VXC+bUPzXTh
// SIG // WZ62gEmUni4Zu7IllS9cPHnmdWHnTKAtPNnbhaRCyc+m
// SIG // 9Fm/aQ9zf1/duEvIdW2cexr9b/zpWt+134B8W94D6o38
// SIG // Rj5caPlz8M8xcJgQJvRqthv3Z0Mla3DOnIGuniB8eWBj
// SIG // VQSlziXgAYQut/YnjCvFPNNb5IzxeFXV044+tiMPTzQh
// SIG // tmovwH4gXREJ2fbr1hesYrpAgeKnOcplwJLyM3fRgAed
// SIG // MlU3lnOzq3/ZiyoEYOq68Np3v3fgUVPDO9Rw7dWgjJ33
// SIG // ddbC8/z9IIVUmHbVbygZBOm0YfKXL4WXiF6dUxVkXW/q
// SIG // iw62KtfwYVOISGd/ydF06DvJlgAnTHL0K0N9tdpOf9x/
// SIG // curc38YJgoWML7mZQIT4AmGbEy4x29JQaYqIAV2I8CNR
// SIG // OqxZYEFkmbR4LCB4YkWaZAD5Xv/3wEpwT6BQvs715ZEN
// SIG // DAp4By+jqvE2/ZjiMqscDpn/CLdr98pSEsI1kRLyoZ2u
// SIG // kMCbuqH7oWNjHK0BuSIozq5M3L9Qs+XC2VhmgAkMNA/t
// SIG // 5gLLDBVs1NsddEFJL41xwLSxIHhbtTrvAgMBAAGjggFJ
// SIG // MIIBRTAdBgNVHQ4EFgQU2TvawYOUfSvkPC98ZHlfAkjw
// SIG // HtswHwYDVR0jBBgwFoAUn6cVXQBeYl2D9OXSZacbUzUZ
// SIG // 6XIwXwYDVR0fBFgwVjBUoFKgUIZOaHR0cDovL3d3dy5t
// SIG // aWNyb3NvZnQuY29tL3BraW9wcy9jcmwvTWljcm9zb2Z0
// SIG // JTIwVGltZS1TdGFtcCUyMFBDQSUyMDIwMTAoMSkuY3Js
// SIG // MGwGCCsGAQUFBwEBBGAwXjBcBggrBgEFBQcwAoZQaHR0
// SIG // cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9jZXJ0
// SIG // cy9NaWNyb3NvZnQlMjBUaW1lLVN0YW1wJTIwUENBJTIw
// SIG // MjAxMCgxKS5jcnQwDAYDVR0TAQH/BAIwADAWBgNVHSUB
// SIG // Af8EDDAKBggrBgEFBQcDCDAOBgNVHQ8BAf8EBAMCB4Aw
// SIG // DQYJKoZIhvcNAQELBQADggIBAGVu7cijKec/PQrWI9t4
// SIG // 2ex6PZvXmXmx2XYUAUEEZP2VF1zaA1XwsAi6w9gceFS9
// SIG // ENyzHiVsw2FUo8a7hMMtqo238Ij5IW0a6p1EulU/VcT1
// SIG // wIvIqso+lwkUkKo+lX55+gC1gGYhRBzHHBPtYhDuBqDz
// SIG // 6uQq+syQKhGopLSYq/wnWwp+Lzn4ba6Fn/VG15JV1hk5
// SIG // k6P5JvjDOidMJOPsS2Aw38Ffflbl1PN3vAl0Z6liRWLz
// SIG // vV1KsLZOvVkXMBHtLjh2sJZmknqmElptU06w3EUkqBLK
// SIG // S6A4ZbNDfXxGvcxM+DazcGez6lQ9WAyKN3htQ4fYGUSw
// SIG // swzA5yiVNNmqDUdit1jWPGlQAj2KmMFBEg0v87vTln79
// SIG // /YuM2YlCigJUlVfbhp2lnnX1Kx9rMaipca33VuaoqjR8
// SIG // jT0iXixQeHiKHqumJAMGXIvu+a8J6PBXFh69jipXBNn+
// SIG // jeC+X5HSUXFhL194gzg14bT5awNnuMtyLkwV643CixBj
// SIG // fPbpeDWiPRT276dxH25NT7EGYnwG2UJ2FDXdE0xfk/6S
// SIG // tFg8HdcKn0mbpdo7X33mrfYAhmbWbMEYjrIeW+JdoQLl
// SIG // PMaI7Ute4+1dlTZf3ehAlsyh7e/z7kI8qtBqUZbJi6HZ
// SIG // rdXWnBuP5bQUSYQU+m7xMj6pg7UghBZRJG8WNe2Hk5vT
// SIG // aEwyMIIHcTCCBVmgAwIBAgITMwAAABXF52ueAptJmQAA
// SIG // AAAAFTANBgkqhkiG9w0BAQsFADCBiDELMAkGA1UEBhMC
// SIG // VVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcT
// SIG // B1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29mdCBDb3Jw
// SIG // b3JhdGlvbjEyMDAGA1UEAxMpTWljcm9zb2Z0IFJvb3Qg
// SIG // Q2VydGlmaWNhdGUgQXV0aG9yaXR5IDIwMTAwHhcNMjEw
// SIG // OTMwMTgyMjI1WhcNMzAwOTMwMTgzMjI1WjB8MQswCQYD
// SIG // VQQGEwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4G
// SIG // A1UEBxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0
// SIG // IENvcnBvcmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQg
// SIG // VGltZS1TdGFtcCBQQ0EgMjAxMDCCAiIwDQYJKoZIhvcN
// SIG // AQEBBQADggIPADCCAgoCggIBAOThpkzntHIhC3miy9ck
// SIG // eb0O1YLT/e6cBwfSqWxOdcjKNVf2AX9sSuDivbk+F2Az
// SIG // /1xPx2b3lVNxWuJ+Slr+uDZnhUYjDLWNE893MsAQGOhg
// SIG // fWpSg0S3po5GawcU88V29YZQ3MFEyHFcUTE3oAo4bo3t
// SIG // 1w/YJlN8OWECesSq/XJprx2rrPY2vjUmZNqYO7oaezOt
// SIG // gFt+jBAcnVL+tuhiJdxqD89d9P6OU8/W7IVWTe/dvI2k
// SIG // 45GPsjksUZzpcGkNyjYtcI4xyDUoveO0hyTD4MmPfrVU
// SIG // j9z6BVWYbWg7mka97aSueik3rMvrg0XnRm7KMtXAhjBc
// SIG // TyziYrLNueKNiOSWrAFKu75xqRdbZ2De+JKRHh09/SDP
// SIG // c31BmkZ1zcRfNN0Sidb9pSB9fvzZnkXftnIv231fgLrb
// SIG // qn427DZM9ituqBJR6L8FA6PRc6ZNN3SUHDSCD/AQ8rdH
// SIG // GO2n6Jl8P0zbr17C89XYcz1DTsEzOUyOArxCaC4Q6oRR
// SIG // RuLRvWoYWmEBc8pnol7XKHYC4jMYctenIPDC+hIK12Nv
// SIG // DMk2ZItboKaDIV1fMHSRlJTYuVD5C4lh8zYGNRiER9vc
// SIG // G9H9stQcxWv2XFJRXRLbJbqvUAV6bMURHXLvjflSxIUX
// SIG // k8A8FdsaN8cIFRg/eKtFtvUeh17aj54WcmnGrnu3tz5q
// SIG // 4i6tAgMBAAGjggHdMIIB2TASBgkrBgEEAYI3FQEEBQID
// SIG // AQABMCMGCSsGAQQBgjcVAgQWBBQqp1L+ZMSavoKRPEY1
// SIG // Kc8Q/y8E7jAdBgNVHQ4EFgQUn6cVXQBeYl2D9OXSZacb
// SIG // UzUZ6XIwXAYDVR0gBFUwUzBRBgwrBgEEAYI3TIN9AQEw
// SIG // QTA/BggrBgEFBQcCARYzaHR0cDovL3d3dy5taWNyb3Nv
// SIG // ZnQuY29tL3BraW9wcy9Eb2NzL1JlcG9zaXRvcnkuaHRt
// SIG // MBMGA1UdJQQMMAoGCCsGAQUFBwMIMBkGCSsGAQQBgjcU
// SIG // AgQMHgoAUwB1AGIAQwBBMAsGA1UdDwQEAwIBhjAPBgNV
// SIG // HRMBAf8EBTADAQH/MB8GA1UdIwQYMBaAFNX2VsuP6KJc
// SIG // YmjRPZSQW9fOmhjEMFYGA1UdHwRPME0wS6BJoEeGRWh0
// SIG // dHA6Ly9jcmwubWljcm9zb2Z0LmNvbS9wa2kvY3JsL3By
// SIG // b2R1Y3RzL01pY1Jvb0NlckF1dF8yMDEwLTA2LTIzLmNy
// SIG // bDBaBggrBgEFBQcBAQROMEwwSgYIKwYBBQUHMAKGPmh0
// SIG // dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2kvY2VydHMv
// SIG // TWljUm9vQ2VyQXV0XzIwMTAtMDYtMjMuY3J0MA0GCSqG
// SIG // SIb3DQEBCwUAA4ICAQCdVX38Kq3hLB9nATEkW+Geckv8
// SIG // qW/qXBS2Pk5HZHixBpOXPTEztTnXwnE2P9pkbHzQdTlt
// SIG // uw8x5MKP+2zRoZQYIu7pZmc6U03dmLq2HnjYNi6cqYJW
// SIG // AAOwBb6J6Gngugnue99qb74py27YP0h1AdkY3m2CDPVt
// SIG // I1TkeFN1JFe53Z/zjj3G82jfZfakVqr3lbYoVSfQJL1A
// SIG // oL8ZthISEV09J+BAljis9/kpicO8F7BUhUKz/AyeixmJ
// SIG // 5/ALaoHCgRlCGVJ1ijbCHcNhcy4sa3tuPywJeBTpkbKp
// SIG // W99Jo3QMvOyRgNI95ko+ZjtPu4b6MhrZlvSP9pEB9s7G
// SIG // dP32THJvEKt1MMU0sHrYUP4KWN1APMdUbZ1jdEgssU5H
// SIG // LcEUBHG/ZPkkvnNtyo4JvbMBV0lUZNlz138eW0QBjloZ
// SIG // kWsNn6Qo3GcZKCS6OEuabvshVGtqRRFHqfG3rsjoiV5P
// SIG // ndLQTHa1V1QJsWkBRH58oWFsc/4Ku+xBZj1p/cvBQUl+
// SIG // fpO+y/g75LcVv7TOPqUxUYS8vwLBgqJ7Fx0ViY1w/ue1
// SIG // 0CgaiQuPNtq6TPmb/wrpNPgkNWcr4A245oyZ1uEi6vAn
// SIG // Qj0llOZ0dFtq0Z4+7X6gMTN9vMvpe784cETRkPHIqzqK
// SIG // Oghif9lwY1NNje6CbaUFEMFxBmoQtB1VM1izoXBm8qGC
// SIG // A00wggI1AgEBMIH5oYHRpIHOMIHLMQswCQYDVQQGEwJV
// SIG // UzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
// SIG // UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBv
// SIG // cmF0aW9uMSUwIwYDVQQLExxNaWNyb3NvZnQgQW1lcmlj
// SIG // YSBPcGVyYXRpb25zMScwJQYDVQQLEx5uU2hpZWxkIFRT
// SIG // UyBFU046MzcwMy0wNUUwLUQ5NDcxJTAjBgNVBAMTHE1p
// SIG // Y3Jvc29mdCBUaW1lLVN0YW1wIFNlcnZpY2WiIwoBATAH
// SIG // BgUrDgMCGgMVAEsgyDU/uw24JemZsfYhdPa1d4QQoIGD
// SIG // MIGApH4wfDELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldh
// SIG // c2hpbmd0b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNV
// SIG // BAoTFU1pY3Jvc29mdCBDb3Jwb3JhdGlvbjEmMCQGA1UE
// SIG // AxMdTWljcm9zb2Z0IFRpbWUtU3RhbXAgUENBIDIwMTAw
// SIG // DQYJKoZIhvcNAQELBQACBQDubStFMCIYDzIwMjYxMDA0
// SIG // MTk0NTA5WhgPMjAyNjEwMDUxOTQ1MDlaMHQwOgYKKwYB
// SIG // BAGEWQoEATEsMCowCgIFAO5tK0UCAQAwBwIBAAICDpww
// SIG // BwIBAAICElkwCgIFAO5ufMUCAQAwNgYKKwYBBAGEWQoE
// SIG // AjEoMCYwDAYKKwYBBAGEWQoDAqAKMAgCAQACAwehIKEK
// SIG // MAgCAQACAwGGoDANBgkqhkiG9w0BAQsFAAOCAQEAhTzM
// SIG // hzV0Y1iIIDKVvKDvalg598nB6Y6SgcG7P8eYVOYHl2Eu
// SIG // 2iKqX0exSrYCBJUTBkv3QaxN/ZaLjEisZ6JhSWcfV62Q
// SIG // uuZJ/kr+U3G5zXQ6V7e92Yh/ImvSH6Q3VJbtg37QTj3g
// SIG // i5hi/lfO7BTecsu2r/EpbDxp80CsMfgJqIvKWjkpLvrE
// SIG // 7qRJcB/qbnuvvHvHKtYgn0cXVJUZzbUydprk16QW3axK
// SIG // QaAIL4ifvlHbtnH5NpUrMQI9oCemNC68MEesvwvRNSm3
// SIG // UbxRtA7+pdfX1HORXX4xS/7zZRmAgtwMvGfwTWkGzDRM
// SIG // dyRkgHXUoJelKoXpG42P+klT6l2kHjGCBA0wggQJAgEB
// SIG // MIGTMHwxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNo
// SIG // aW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQK
// SIG // ExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xJjAkBgNVBAMT
// SIG // HU1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQSAyMDEwAhMz
// SIG // AAACHzpwaeSiMC6VAAEAAAIfMA0GCWCGSAFlAwQCAQUA
// SIG // oIIBSjAaBgkqhkiG9w0BCQMxDQYLKoZIhvcNAQkQAQQw
// SIG // LwYJKoZIhvcNAQkEMSIEIDLIkbVI6nUn1J/fbhlBXM7a
// SIG // QR9H0gR/Ds3zsWCA8aSqMIH6BgsqhkiG9w0BCRACLzGB
// SIG // 6jCB5zCB5DCBvQQgsCQK31aQKwy1RGQW7pNjQ/dRd1Gc
// SIG // KJi49mF7fKQt/BQwgZgwgYCkfjB8MQswCQYDVQQGEwJV
// SIG // UzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UEBxMH
// SIG // UmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENvcnBv
// SIG // cmF0aW9uMSYwJAYDVQQDEx1NaWNyb3NvZnQgVGltZS1T
// SIG // dGFtcCBQQ0EgMjAxMAITMwAAAh86cGnkojAulQABAAAC
// SIG // HzAiBCBif7lfvfJHsebMhTRNdCV2Y2vNvBS3wpWJ3hsA
// SIG // FJUGsTANBgkqhkiG9w0BAQsFAASCAgDFkl8kWLGOYYyI
// SIG // afXkkoKvXCb+nrruT684+CH3oP5sNV1rml2xtWEE+oe7
// SIG // WOf0i61HZdJPQnDnTCz+SX9CDugmOPWNOBGwpLGhqGcY
// SIG // KwxlfojCbFyFCBEpXzNGcaKGaMZWv5bRtfb9kuYdbNVp
// SIG // JGE+Y7mI1xZUor0EjjvTdT5G4yZnNmU9xEzPpOvLcr1j
// SIG // AceCkogGL2j/WvEz15JzuEx4FD3tLYatB9pA0J7qC1IN
// SIG // HKyg/LHlvPSF1p2x8/wG6er+fM3Mtis7cupCgj1/Vc8U
// SIG // MrUsAK0NmPW5o6sHBVXv+9gt36Tj/Z1SnY+050iQD5SB
// SIG // oq5bY4laaepe66vd96AG5tEkOy0aS2bqA/uEQXWn+1uO
// SIG // 3iHVmxy4zsqKUAMUbnhrxb3ieQwekGhMcqjyUNLPcxWG
// SIG // G/xMqiYNFMLXufnvNdl/J6G/mbBbq5HpFV2tgJzfcb47
// SIG // 5pR+ONO2SDZGZViL+a0oHQ9mv3DOyTumpUCsr2j2a8uk
// SIG // lCR6f76vKqK3bPwMaRDvK1XqALcd2eZgwOHNgqkv1Wfs
// SIG // QaUbuXvJSrvVySHUOZYy9hbAndG8DgPzDqjcJoaZllmW
// SIG // gn/yD4lTKDYrkbGKak7RFe5CJK1wsqZ+F1gJr4IxwBBC
// SIG // U2Ypo4jRgGjdx/jyO6VRAGdKukvhGXow+Y4S1rrUWo8k
// SIG // 7AKZ0mEjGA==
// SIG // End signature block
