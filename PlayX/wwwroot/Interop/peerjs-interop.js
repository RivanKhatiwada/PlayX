window.peerManager = {
    peer: null,
    dataConn: null,
    dotNetRef: null,

    initPeer: function (id, dotNetObj) {
        this.dotNetRef = dotNetObj;

        if (id) {
            this.peer = new Peer(id);
        } else {
            this.peer = new Peer();
        }

        return new Promise((resolve) => {
            this.peer.on('open', (assignedId) => {
                resolve(assignedId);
            });

            this.peer.on('connection', (conn) => {
                this.dataConn = conn;
                this.setupConnectionEvents();
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnPeerConnectedCallback', conn.peer);
                }
            });

            this.peer.on('error', (err) => {
                console.error("PeerJS error:", err);
            });
        });
    },

    connectToPeer: function (peerId, dotNetObj) {
        this.dotNetRef = dotNetObj;
        this.dataConn = this.peer.connect(peerId);

        this.dataConn.on('open', () => {
            this.setupConnectionEvents();
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnPeerConnectedCallback', peerId);
            }
        });

        this.dataConn.on('error', (err) => {
            console.error("Connection error:", err);
        });
    },

    setupConnectionEvents: function () {
        this.dataConn.on('data', (data) => {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnDataReceivedCallback', data);
            }
        });

        this.dataConn.on('close', () => {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnPeerDisconnectedCallback');
            }
        });
    },

    sendData: function (message) {
        if (this.dataConn && this.dataConn.open) {
            this.dataConn.send(message);
        }
    },

    disconnect: function () {
        if (this.dataConn) {
            this.dataConn.close();
        }
        if (this.peer) {
            this.peer.destroy();
        }
        this.dataConn = null;
        this.peer = null;
    }
};