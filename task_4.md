# Завдання 4

Для підпису CSR потрібен приватний ключ іншої команди. У наявних файлах є лише `D:\Downloads\key.pub`, тобто публічний ключ; ним можна перевіряти підпис або шифрувати, але неможливо створити підпис сертифіката.

Що безпечно передавати іншій команді:

```powershell
keys_task3\server.csr
```

Що не можна передавати:

```powershell
keys_task3\server.key
```

Команда, яку має виконати інша команда зі своїм приватним ключем:

```powershell
& "C:\Program Files\Git\mingw64\bin\openssl.exe" x509 -req -sha256 `
  -in keys_task3\server.csr `
  -signkey other_team_private.key `
  -out task4_signed_by_other_team.crt
```

Після отримання сертифіката його можна перевірити так:

```powershell
& "C:\Program Files\Git\mingw64\bin\openssl.exe" x509 -text -noout -in task4_signed_by_other_team.crt
```

Пояснення для звіту: ми передаємо тільки CSR, бо він містить публічний ключ і метадані власника, але не містить приватного ключа. Якщо передати `server.key`, інша команда зможе підписувати дані від нашого імені.
