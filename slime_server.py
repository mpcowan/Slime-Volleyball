from time import sleep
from socket import *
import thread

BUFF = 2048
HOST = '0.0.0.0'
PORT = 9001

availableKeys = [ 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 ]
activeKeyTable = {}

def createSessionKey(addr):
  if len(availableKeys) != 0:
    key = availableKeys.pop()
    activeKeyTable[key] = (str(addr[0]), None)
    return key
  else:
    return None

def joinSession(key, addr):
  if key in activeKeyTable:
    creator, joiner = activeKeyTable[key]
    if joiner is None:
      activeKeyTable[key] = (creator, str(addr[0]))
      return True
    else:
      return False
  else:
    return False

def hasOpponent(key):
  if key in activeKeyTable:
    creator, joiner = activeKeyTable[key]
    if joiner is None:
      return False
    else:
      return True
  else:
    return False

def releaseSessionKey(key):
  if key in activeKeyTable:
    del activeKeyTable[key]
    availableKeys.append(key)

def handler(clientsock,addr):
  client_key = 0
  while True:
    data = clientsock.recv(BUFF)
    if not data:
      break
    print repr(addr) + ' recv:' + repr(data)
    if str(data) == "create":
      client_key = createSessionKey(addr)
      if client_key is None:
        print "To creator: ERROR"
        clientsock.send("ERROR")
      else:
        clientsock.send(str(client_key))
        print "To Creator: " + str(client_key)
        while True:
          sleep(1)
          if hasOpponent(client_key):
            print "To creator: " + activeKeyTable[client_key][1]
            clientsock.send(activeKeyTable[client_key][1])
            break
    elif str(data).startswith("join"):
      try:
        client_key = int(str(data).split(" ")[-1])
        if joinSession(client_key, addr):
          print "To joiner: " + activeKeyTable[client_key][0]
          clientsock.send(activeKeyTable[client_key][0])
        else:
          clientsock.send("ERROR")
      except:
        clientsock.send("ERROR")
    print repr(addr) + ' sent: ' + str(data)
    if "close" == data.rstrip():
      break
  clientsock.close()
  sleep(10)
  releaseSessionKey(client_key)
  print addr, "- closed connection" #log on console

if __name__=='__main__':
    ADDR = (HOST, PORT)
    serversock = socket(AF_INET, SOCK_STREAM)
    serversock.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
    serversock.bind(ADDR)
    serversock.listen(5)
    while 1:
        print 'waiting for connection... listening on port', PORT
        clientsock, addr = serversock.accept()
        print '...connected from:', addr
        thread.start_new_thread(handler, (clientsock, addr))
