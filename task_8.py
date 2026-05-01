import base64
from task_1 import sha256

def calculate_certificate_fingerprint():
    with open("kse_cert.crt", 'r') as file:
        lines = file.readlines()

    base64_data = ""
    for line in lines:
        clean_line = line.strip()
        if "BEGIN CERTIFICATE" not in clean_line and "END CERTIFICATE" not in clean_line and clean_line != "":
            base64_data += clean_line
    der_bytes = base64.b64decode(base64_data)
    hex_hash = sha256(bytearray(der_bytes))
    fingerprint = ':'.join(hex_hash[i:i + 2].upper() for i in range(0, len(hex_hash), 2))
    return fingerprint

if __name__ == '__main__':
    result = calculate_certificate_fingerprint()
    print("Розрахований відбиток сертифікату:")
    print(result)