from time import time, sleep
from socket import *
import thread

BUFF = 2048
HOST = '0.0.0.0'
PORT = 9001

availableKeys = [ 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 ]
activeKeyTable = {}

def createSessionKey(clientsock):
  if len(availableKeys) != 0:
    key = availableKeys.pop()
    activeKeyTable[key] = (clientsock, None)
    return key
  else:
    return None

def joinSession(key, clientsock):
  if key in activeKeyTable:
    creator, joiner = activeKeyTable[key]
    if joiner is None:
      activeKeyTable[key] = (creator, clientsock)
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

def forwardData(key, data, is_creator):
  if key in activeKeyTable:
    creator, joiner = activeKeyTable[key]
    if is_creator and joiner != None:
      joiner.send(data)
    elif not is_creator and creator != None:
      creator.send(data)

def releaseSessionKey(key):
  if key in activeKeyTable:
    del activeKeyTable[key]
    availableKeys.append(key)

def handler(clientsock,addr):
  client_key = 0
  creator = False
  while 1:
    data = clientsock.recv(BUFF)
    if not data:
      break
    print repr(addr) + ' recv:' + repr(data)
    if str(data) == "create":
      client_key = createSessionKey(clientsock)
      if client_key is None:
        clientsock.send("ERROR")
      else:
        creator = True
        clientsock.send(str(client_key))
        while 1:
          sleep(2)
          if hasOpponent(client_key):
            clientsock.send("ready")
            break
    elif str(data).startswith("join"):
      try:
        client_key = int(str(data).split(" ")[-1])
        if joinSession(client_key, clientsock):
          clientsock.send("SUCCESS")
        else:
          clientsock.send("ERROR")
      except:
        clientsock.send("ERROR")
    else:
      forwardData(client_key, data, creator)
    print repr(addr) + ' sent:' + repr(response(data))
    if "close" == data.rstrip():
      break
  clientsock.close()
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
