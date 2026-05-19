import socket
import time
import argparse

# ── Налаштування ────────────────────────────────────────────
HOST = "127.0.0.1"     # IP сервера (замінити на реальний)
PORT = 5201
RECV_BUFFER = 256 * 1024  # 256 КБ — буфер прийому
# ────────────────────────────────────────────────────────────


def format_speed(bytes_per_sec: float) -> str:
    """Форматує швидкість у читабельний вигляд (біт/с та байт/с)."""
    bps = bytes_per_sec * 8
    for unit in ["біт/с", "Кбіт/с", "Мбіт/с", "Гбіт/с"]:
        if bps < 1024:
            break
        bps /= 1024
    Bps = bytes_per_sec
    for Bunit in ["Б/с", "КБ/с", "МБ/с", "ГБ/с"]:
        if Bps < 1024:
            break
        Bps /= 1024
    return f"{bps:.2f} {unit}  ({Bps:.2f} {Bunit})"


def run_client(host: str, port: int):
    print(f"[Клієнт] Підключаюсь до {host}:{port} ...")

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.connect((host, port))
        print(f"[Клієнт] З'єднання встановлено!")
        print("-" * 55)

        # ── Крок 1: надіслати команду START ─────────────────
        s.sendall(b"START")

        # ── Крок 2: приймати дані, рахувати байти і час ─────
        total_bytes = 0
        t_start = time.perf_counter()

        # Для живого відображення прогресу
        last_report = t_start
        last_bytes = 0

        print("[Клієнт] Отримую дані від сервера...\n")
        print(f"  {'Час':>6}  {'Отримано':>10}  {'Поточна швидкість':>30}")
        print(f"  {'-'*6}  {'-'*10}  {'-'*30}")

        while True:
            chunk = s.recv(RECV_BUFFER)

            if not chunk:
                # З'єднання закрите сервером
                break

            # Перевіряємо, чи не прийшов сигнал DONE в кінці чанку
            if chunk.endswith(b"DONE"):
                # Відкидаємо останні 4 байти сигналу
                real_data = chunk[:-4]
                total_bytes += len(real_data)
                break

            total_bytes += len(chunk)

            # Виводимо прогрес кожну секунду
            now = time.perf_counter()
            if now - last_report >= 1.0:
                interval_bytes = total_bytes - last_bytes
                interval_time = now - last_report
                instant_speed = interval_bytes / interval_time if interval_time > 0 else 0

                elapsed_total = now - t_start
                received_mb = total_bytes / 1_048_576

                print(f"  {elapsed_total:>5.1f}с  {received_mb:>8.1f} МБ  {format_speed(instant_speed):>30}")

                last_report = now
                last_bytes = total_bytes

        # ── Крок 3: фінальний результат ──────────────────────
        elapsed = time.perf_counter() - t_start

        print()
        print("=" * 55)
        print("  ПІДСУМОК")
        print("=" * 55)
        print(f"  Отримано  : {total_bytes / 1_048_576:.2f} МБ  ({total_bytes:,} байт)")
        print(f"  Час       : {elapsed:.3f} с")
        print(f"  Швидкість : {format_speed(total_bytes / elapsed)}")
        print("=" * 55)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="TCP Speed Test — Клієнт")
    parser.add_argument("--host", type=str, default=HOST, help="IP або hostname сервера")
    parser.add_argument("--port", type=int, default=PORT, help="Порт (default: 5201)")
    parser.add_argument("--buffer", type=int, default=RECV_BUFFER, help="Розмір буфера прийому в байтах")
    args = parser.parse_args()

    run_client(args.host, args.port)