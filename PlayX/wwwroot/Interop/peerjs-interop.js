window.peerManager = {
    peer: null,
    dataConn: null,       // Client's single connection to the host
    clientConns: [],      // Host's list of connections coming from clients
    dotNetRef: null,

    initPeer: function (id, dotNetObj) {
        this.dotNetRef = dotNetObj;
        this.clientConns = [];

        if (this.peer) {
            this.peer.destroy();
        }

        this.peer = id ? new Peer(id) : new Peer();

        return new Promise((resolve) => {
            this.peer.on('open', (assignedId) => {
                resolve(assignedId);
            });

            // HOST SIDE: triggered when a client connects to this room code
            this.peer.on('connection', (conn) => {
                this.clientConns.push(conn);
                this.setupHostConnectionEvents(conn);

                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnPeerConnectedCallback', conn.peer);
                }

                conn.on('close', () => {
                    this.clientConns = this.clientConns.filter(c => c !== conn);
                    if (this.dotNetRef) {
                        this.dotNetRef.invokeMethodAsync('OnPeerDisconnectedCallback');
                    }
                });
            });

            this.peer.on('error', (err) => {
                console.error("PeerJS error:", err);
            });
        });
    },

    connectToPeer: function (peerId, dotNetObj) {
        this.dotNetRef = dotNetObj;

        if (!this.peer) {
            console.error("Peer not initialized before connecting.");
            return;
        }

        // CLIENT SIDE: connect to the host's peer ID
        this.dataConn = this.peer.connect(peerId);

        this.dataConn.on('open', () => {
            this.setupClientConnectionEvents(this.dataConn);
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnPeerConnectedCallback', peerId);
            }
        });

        this.dataConn.on('error', (err) => {
            console.error("Connection error:", err);
        });

        this.dataConn.on('close', () => {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnPeerDisconnectedCallback');
            }
        });
    },

    setupHostConnectionEvents: function (conn) {
        conn.on('data', (data) => {
            // AUTO-RELAY / BROADCAST: 
            // When host receives a message from one client, broadcast it to all OTHER connected clients
            this.clientConns.forEach(c => {
                if (c !== conn && c.open) {
                    c.send(data);
                }
            });

            // Notify the host's C# code so the host sees the message too
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnDataReceivedCallback', data);
            }
        });
    },

    setupClientConnectionEvents: function (conn) {
        conn.on('data', (data) => {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnDataReceivedCallback', data);
            }
        });
    },

    sendData: function (message) {
        // If caller is a Client, send message to the Host
        if (this.dataConn && this.dataConn.open) {
            this.dataConn.send(message);
        }

        // If caller is the Host, broadcast message to ALL connected clients
        if (this.clientConns && this.clientConns.length > 0) {
            this.clientConns.forEach(conn => {
                if (conn.open) {
                    conn.send(message);
                }
            });
        }
    },

    disconnect: function () {
        if (this.dataConn) {
            this.dataConn.close();
        }
        if (this.clientConns) {
            this.clientConns.forEach(c => c.close());
        }
        if (this.peer) {
            this.peer.destroy();
        }
        this.dataConn = null;
        this.clientConns = [];
        this.peer = null;
    }
};