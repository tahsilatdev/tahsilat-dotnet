<#
.SYNOPSIS
    Sends a self-signed test webhook to the sample app, the same way Tahsilat does.

.DESCRIPTION
    The sandbox never runs automatic recurring charges ("Test ortamında otomatik
    tahsilat yapılmaz"), so the only way to exercise the recurring-cycle path of
    the webhook handler is to craft the payload yourself and sign it with the
    webhook secret: HMAC-SHA256 over "<unix timestamp>.<payload>", sent in the
    X-Tahsilat-Signature header as "t=<timestamp>,v1=<hex>".

    This tests the SDK's signature verification, replay protection and payload
    parsing plus the sample app's handling. It does NOT touch the Tahsilat API.

.PARAMETER Secret
    Webhook secret. Defaults to Tahsilat:WebhookSecret from appsettings.local.json
    next to this script's project.

.PARAMETER Url
    Webhook endpoint. Defaults to the sample app's https://localhost:7195/webhook/receive.

.PARAMETER Cycle
    cycle_number to send. 0 = initial payment, 1+ = recurring charge.

.PARAMETER AgeSeconds
    How old the timestamp should be. > 300 makes the SDK reject it (replay protection).

.PARAMETER Fail
    Send a failed charge (payment_status 2, subscription past_due).

.EXAMPLE
    .\Send-TestWebhook.ps1 -Cycle 1
    .\Send-TestWebhook.ps1 -Cycle 2 -Fail
    .\Send-TestWebhook.ps1 -Cycle 1 -AgeSeconds 600   # expect 400: too old
#>
param(
    [string] $Secret,
    [string] $Url = 'https://localhost:7195/webhook/receive',
    [int] $Cycle = 1,
    [int] $AgeSeconds = 0,
    [switch] $Fail,
    [long] $SubscriptionId = 57391699178136,
    [int] $Amount = 7895000
)

$ErrorActionPreference = 'Stop'

if (-not $Secret) {
    $settings = Join-Path $PSScriptRoot '..\appsettings.local.json'
    if (-not (Test-Path $settings)) { throw "Secret verilmedi ve $settings bulunamadı." }
    $Secret = (Get-Content $settings -Raw | ConvertFrom-Json).Tahsilat.WebhookSecret
    if (-not $Secret) { throw 'appsettings.local.json içinde Tahsilat:WebhookSecret yok.' }
}

# Random 14 digit transaction id, like the real ones.
$transactionId = [long](Get-Random -Minimum 10000000000000 -Maximum 99999999999999)
$now = [DateTimeOffset]::Now
$isoNow = $now.ToString("yyyy-MM-dd'T'HH:mm:sszzz")
$nextBilling = $now.AddDays(1).ToString("yyyy-MM-dd'T'HH:mm:sszzz")

$payload = [ordered]@{
    transaction_id          = $transactionId
    amount                  = $Amount
    currency_code           = 'TRY'
    installment_count       = 1
    payment_status          = $(if ($Fail) { 2 } else { 1 })
    payment_status_text     = $(if ($Fail) { 'fail' } else { 'success' })
    transaction_status      = $(if ($Fail) { 4 } else { 2 })
    transaction_status_text = $(if ($Fail) { 'cancelled' } else { 'completed' })
    transaction_message     = $(if ($Fail) { 'Abonelik tahsilatı başarısız: yetersiz bakiye.' } else { 'Abonelik tahsilatı başarıyla tamamlandı.' })
    transaction_code        = $(if ($Fail) { 'fail' } else { 'success' })
    payment_method          = 2
    payment_method_text     = 'is_2d'
    pre_auth                = $false
    created_at              = $isoNow
    start_at                = $isoNow
    end_at                  = $isoNow
    metadata                = @(@{ key = 'source'; value = 'Send-TestWebhook.ps1' })
    subscription            = [ordered]@{
        subscription_id    = $SubscriptionId
        status             = $(if ($Fail) { 2 } else { 1 })
        cycle_number       = $Cycle
        next_billing_at    = $nextBilling
        cancelled_at       = $null
        status_text        = $(if ($Fail) { 'past_due' } else { 'active' })
        is_initial_payment = ($Cycle -eq 0)
    }
    formatted_amount        = ($Amount / 100).ToString('N2', [Globalization.CultureInfo]::InvariantCulture)
}

$json = $payload | ConvertTo-Json -Depth 5 -Compress
$timestamp = $now.ToUnixTimeSeconds() - $AgeSeconds

$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($Secret))
$hash = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes("$timestamp.$json"))
$signature = ($hash | ForEach-Object { $_.ToString('x2') }) -join ''

Write-Host "POST $Url"
Write-Host "transaction_id=$transactionId cycle=$Cycle fail=$($Fail.IsPresent) timestamp age=${AgeSeconds}s"

# Windows PowerShell 5.1: accept the ASP.NET dev certificate.
if ($PSVersionTable.PSEdition -eq 'Desktop') {
    [Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
}

try {
    $response = Invoke-WebRequest -Method Post -Uri $Url -Body ([Text.Encoding]::UTF8.GetBytes($json)) `
        -ContentType 'application/json' -Headers @{ 'X-Tahsilat-Signature' = "t=$timestamp,v1=$signature" } -UseBasicParsing
    Write-Host "HTTP $($response.StatusCode): $($response.Content)"
}
catch {
    $resp = $_.Exception.Response
    if ($resp) {
        $reader = New-Object IO.StreamReader ($resp.GetResponseStream())
        Write-Host "HTTP $([int]$resp.StatusCode): $($reader.ReadToEnd())"
    }
    else { throw }
}
