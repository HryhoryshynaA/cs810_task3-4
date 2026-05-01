import struct

K = [
    0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
    0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
    0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
    0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
    0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
    0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
    0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
    0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
    0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
    0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
    0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
    0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
    0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
    0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
    0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
    0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2,
]

H0 = [
    0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
    0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19,
]

def rotate_right(x, n):
    return ((x >> n) | (x << (32 - n))) & 0xFFFFFFFF

def shr(x, n):
    return x >> n

def ch(x, y, z):
    return ((x & y) ^ (~x & z)) & 0xFFFFFFFF

def maj(x, y, z):
    return (x & y) ^ (x & z) ^ (y & z)

def sigma0_upper(x):
    return rotate_right(x, 2)  ^ rotate_right(x, 13) ^ rotate_right(x, 22)
def sigma1_upper(x):
    return rotate_right(x, 6)  ^ rotate_right(x, 11) ^ rotate_right(x, 25)


def sigma0_lower(x):
    return rotate_right(x, 7)  ^ rotate_right(x, 18) ^ shr(x, 3)
def sigma1_lower(x):
    return rotate_right(x, 17) ^ rotate_right(x, 19) ^ shr(x, 10)

def pad(message) -> bytes:
    bit_length = len(message) * 8
    message += b'\x80'
    zeros = (55 - len(message) + 1) % 64
    message += b'\x00' * zeros
    message += struct.pack('>Q', bit_length)
    return message


def sha256(message):
    H = list(H0)
    padded_mes = pad(message)

    for block_start in range(0, len(padded_mes), 64):
        block = padded_mes[block_start : block_start + 64]
        W = list(struct.unpack('>16I', block))

        for t in range(16, 64):
            w = (sigma1_lower(W[t - 2]) + W[t - 7] + sigma0_lower(W[t - 15]) + W[t - 16]) & 0xFFFFFFFF
            W.append(w)

        a, b, c, d, e, f, g, h = H
        for t in range(64):
            T1 = (h + sigma1_upper(e) + ch(e, f, g) + K[t] + W[t]) & 0xFFFFFFFF
            T2 = (sigma0_upper(a) + maj(a, b, c)) & 0xFFFFFFFF
            h = g
            g = f
            f = e
            e = (d + T1) & 0xFFFFFFFF
            d = c
            c = b
            b = a
            a = (T1 + T2) & 0xFFFFFFFF

        H[0] = (H[0] + a) & 0xFFFFFFFF
        H[1] = (H[1] + b) & 0xFFFFFFFF
        H[2] = (H[2] + c) & 0xFFFFFFFF
        H[3] = (H[3] + d) & 0xFFFFFFFF
        H[4] = (H[4] + e) & 0xFFFFFFFF
        H[5] = (H[5] + f) & 0xFFFFFFFF
        H[6] = (H[6] + g) & 0xFFFFFFFF
        H[7] = (H[7] + h) & 0xFFFFFFFF

    return ''.join(f'{x:08x}' for x in H)

def run_tests():
    print("tests for SHA-256\n")

    #taken from json file (https://di-mgt.com.au/sha_testvectors.json) and asked gemini to rewrite a little to make the covenient structure for tests (https://gemini.google.com/share/30cade75ea80)
    test_vectors = [
        {
            "description": "'abc', the bit string (0x)616263 of length 24 bits",
            "message": b"abc",
            "expected": "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"
        },
        {
            "description": "the empty string '', a bit string of length 0",
            "message": b"",
            "expected": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
        },
        {
            "description": "length 448 bits",
            "message": b"abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq",
            "expected": "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1"
        },
        {
            "description": "length 896 bits",
            "message": b"abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu",
            "expected": "cf5b16a778af8380036ce59e7b0492370b249b11e8f07a51afac45037afee9d1"
        },
        {
            "description": "one million (1,000,000) repetitions of the character 'a' (0x61)",
            "message": b"a" * 1000000,
            "expected": "cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0"
        }
    ]

    all_passed = True
    for idx, test in enumerate(test_vectors, 1):
        result = sha256(test["message"])
        passed = result == test["expected"]
        status = "PASSED" if passed else "ERROR"
        if not passed:
            all_passed = False

        print(f"Test {idx}: {test['description']}")
        print(f"Expected: {test['expected']}")
        print(f"Received: {result}")
        print(status + "\n")

    if all_passed:
        print("All tests passed")

run_tests()