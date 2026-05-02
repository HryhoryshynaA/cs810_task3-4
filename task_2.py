from task_1 import sha256


MESSAGE = b"give my friend 2 bitcoins for a pizza"
PREFIX = bytes.fromhex("63733831302d7461736b322d000000002c3fb5a2")


def main():
    digest = sha256(PREFIX + MESSAGE)

    print(f"Message: {MESSAGE.decode('ascii')}")
    print(f"Prefix length: {len(PREFIX)} bytes")
    print(f"Prefix hex: {PREFIX.hex()}")
    print(r"Prefix escaped: cs810-task2-\x00\x00\x00\x00,?\xb5\xa2")
    print(f"SHA-256(prefix || message): {digest}")
    print(f"First 32 bits: {digest[:8]}")
    print("Check:", "PASSED" if digest.startswith("00000000") else "FAILED")


if __name__ == "__main__":
    main()
