import socket

# Create a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind the socket to the port
server_address = ('0.0.0.0', 9002)
sock.bind(server_address)

routing_table = {}

while True:
  data, address = sock.recvfrom(1024)
  if data:
    raw_data = str(data).strip()
    print address[0] + " - " + raw_data
    data_parts = raw_data.split(' ')
    if raw_data == "receiver initializing":
      routing_table[address[0]] = address[1]
    if len(data_parts) >= 5:
      gameID = data_parts[0]
      dest_ip = data_parts[1]
      if dest_ip == "self":
        dest_ip = address[0]
      if dest_ip in routing_table:
        sendData = " ".join(data_parts[2:])
        prove_send = sock.sendto(sendData, (dest_ip, routing_table[dest_ip]))
        print "Sent " + str(prove_send) + " bytes to " + dest_ip + ":" + str(routing_table[dest_ip])
      else:
        print "Unknown port for specified ip"