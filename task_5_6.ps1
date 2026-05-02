$ErrorActionPreference = "Stop"

$openssl = "C:\Program Files\Git\mingw64\bin\openssl.exe"
$message = "give my friend 2 bitcoins for a pizza"
$messagePath = Join-Path $PSScriptRoot "task_message.bin"
$hashPath = Join-Path $PSScriptRoot "task5_hash.bin"
$privateKey = Join-Path $PSScriptRoot "keys_task3\server.key"
$publicKey = Join-Path $PSScriptRoot "keys_task3\server.pub"
$textbookInput = Join-Path $PSScriptRoot "task5_hash_padded_8192.bin"
$textbookSignature = Join-Path $PSScriptRoot "task5_textbook.sig"
$textbookRecovered = Join-Path $PSScriptRoot "task5_textbook_recovered.bin"
$pssSignature = Join-Path $PSScriptRoot "task5_pss.sig"
$encryptionKey = Join-Path $PSScriptRoot "task6_key.pub"
$ciphertext = Join-Path $PSScriptRoot "task6_ciphertext.bin"

[System.IO.File]::WriteAllBytes($messagePath, [System.Text.Encoding]::ASCII.GetBytes($message))

& $openssl dgst -sha256 -binary -out $hashPath $messagePath
& $openssl pkey -in $privateKey -pubout -out $publicKey

$hash = [System.IO.File]::ReadAllBytes($hashPath)
$padded = New-Object byte[] 1024
[Array]::Copy($hash, 0, $padded, 1024 - $hash.Length, $hash.Length)
[System.IO.File]::WriteAllBytes($textbookInput, $padded)

& $openssl rsautl -sign -raw -inkey $privateKey -in $textbookInput -out $textbookSignature
& $openssl rsautl -verify -raw -pubin -inkey $publicKey -in $textbookSignature -out $textbookRecovered

& $openssl dgst -sha256 -sign $privateKey `
    -sigopt rsa_padding_mode:pss `
    -sigopt rsa_pss_saltlen:32 `
    -out $pssSignature `
    $messagePath
& $openssl dgst -sha256 -verify $publicKey `
    -signature $pssSignature `
    -sigopt rsa_padding_mode:pss `
    -sigopt rsa_pss_saltlen:32 `
    $messagePath

& $openssl pkeyutl -encrypt `
    -pubin `
    -inkey $encryptionKey `
    -in $messagePath `
    -out $ciphertext `
    -pkeyopt rsa_padding_mode:oaep `
    -pkeyopt rsa_oaep_md:sha256 `
    -pkeyopt rsa_mgf1_md:sha256

& $openssl base64 -A -in $textbookSignature -out "$textbookSignature.b64"
& $openssl base64 -A -in $pssSignature -out "$pssSignature.b64"
& $openssl base64 -A -in $ciphertext -out "$ciphertext.b64"

Write-Host "SHA-256:"
& $openssl dgst -sha256 $messagePath
Write-Host "Created textbook RSA signature, RSA-PSS signature, and RSA-OAEP ciphertext."
