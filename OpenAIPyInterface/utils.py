import time
def receive_data(client_socket):
    data_length = client_socket.recv(4)
    if not data_length: return None
    data_length = int.from_bytes(data_length, byteorder='little')
    data = client_socket.recv(data_length)
    while len(data) < data_length: data += client_socket.recv(data_length - len(data))
    return data.decode('ascii')

def send_data(client_socket, data):
    data_length = len(data).to_bytes(4, byteorder='little')
    client_socket.send(data_length + data.encode('ascii'))

def log(file_name, message):
    if len(message) > 100: print(f'[{file_name}]', message[:50], '...', message[-50:])
    else: print(f'[{file_name}]', message)
    with open(f'{file_name}.txt', 'a') as f: f.write(f'[{time.strftime("%H:%M:%S", time.localtime())}] {message}\n')