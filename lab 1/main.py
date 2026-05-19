import subprocess
import ftplib
import socket
import struct
import math
import re

def ip_to_int(ip_str):
    return struct.unpack("!I", socket.inet_aton(ip_str))[0]

def extract_ip(text):
    match = re.search(r'\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b', text)
    return match.group(0)

# знаходження ІР
print("1: Отримання IP через curl 2ір.ua ")
result = subprocess.run(['curl', '2ip.ua'], capture_output=True, text=True, timeout=10)
raw_output = result.stdout.strip()
ip = extract_ip(raw_output)
print(f"IP-адреса: {ip}")

ip_int = ip_to_int(ip)

# знаходження делегації
print("\n2: Завантаження файлу делегацій")
ftp_host = "ftp.ripe.net"
ftp_filepath = "pub/stats/ripencc/delegated-ripencc-latest"

delegations = []

ftp = ftplib.FTP(ftp_host)
ftp.login()
ftp.retrlines(f'RETR {ftp_filepath}', delegations.append)
ftp.quit()
print(f"Завантажено {len(delegations)} рядків.")

print("Пошук мережі.\n")

found = False
for line in delegations:
    if line.startswith('#') or not line.strip() or line.startswith('ripencc|*'):
        continue

    parts = line.split('|')

    if len(parts) >= 5 and parts[2] == 'ipv4':
        net_ip_str = parts[3]
        length_str = parts[4]
        length = int(length_str)
        if length <= 0:
            continue

        net_ip_int = ip_to_int(net_ip_str)
        host_bits = int(math.log2(length))
        mask = (0xFFFFFFFF << host_bits) & 0xFFFFFFFF

        if (ip_int & mask) == (net_ip_int & mask):
            print("Результат:")
            print(f"IP:  {ip}")
            print(f"Рядок делегації: {line}")
            found = True
            break

if not found:
    print("IP-адресу не знайдено.")