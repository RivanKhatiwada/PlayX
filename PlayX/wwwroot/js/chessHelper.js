window.chessManager = {
    game: null,
    dotNetRef: null,
    boardConfig: null,
    selectedSquare: null,
    possibleMoves: [],
    pendingPromotion: null, // Holds { from, to } if waiting for piece choice

    init: function (dotNetObj, config) {
        this.dotNetRef = dotNetObj;
        this.boardConfig = config;
        this.game = new Chess();
        this.selectedSquare = null;
        this.possibleMoves = [];
        this.pendingPromotion = null;

        this.renderBoard();
    },

    loadFen: function (fen) {
        if (this.game) {
            this.game.load(fen);
            this.selectedSquare = null;
            this.possibleMoves = [];
            this.pendingPromotion = null;
            this.renderBoard();
        }
    },

    renderBoard: function () {
        const container = document.getElementById("chess-board-container");
        if (!container) return;
        container.innerHTML = "";

        const isFlipped = this.boardConfig.isOnline && !this.boardConfig.isHost;

        let ranks = [0, 1, 2, 3, 4, 5, 6, 7];
        let files = [0, 1, 2, 3, 4, 5, 6, 7];
        if (isFlipped) {
            ranks.reverse();
            files.reverse();
        }

        const boardGrid = document.createElement("div");
        boardGrid.className = "chess-grid";

        for (let r of ranks) {
            for (let f of files) {
                const squareName = String.fromCharCode(97 + f) + (8 - r);
                const squareDiv = document.createElement("div");
                const isDark = (r + f) % 2 === 1;

                squareDiv.className = `chess-square ${isDark ? 'dark' : 'light'}`;
                squareDiv.dataset.square = squareName;

                if (this.possibleMoves.includes(squareName)) {
                    squareDiv.classList.add("highlight-move");
                }
                if (this.selectedSquare === squareName) {
                    squareDiv.classList.add("selected-square");
                }

                const piece = this.game.get(squareName);
                if (piece) {
                    const pieceSpan = document.createElement("span");
                    pieceSpan.className = `chess-piece ${piece.color === 'w' ? 'white-piece' : 'black-piece'}`;
                    pieceSpan.innerText = this.getPieceSymbol(piece);
                    squareDiv.appendChild(pieceSpan);
                }

                squareDiv.addEventListener("click", () => this.handleSquareClick(squareName));
                boardGrid.appendChild(squareDiv);
            }
        }
        container.appendChild(boardGrid);
    },

    handleSquareClick: function (square) {
        // If we are currently waiting for a promotion choice, ignore board clicks
        if (this.pendingPromotion) return;

        if (this.boardConfig.isOnline) {
            const myTurn = this.boardConfig.isHost ? (this.game.turn() === 'w') : (this.game.turn() === 'b');
            if (!myTurn) return;
        }

        if (this.selectedSquare === null) {
            const piece = this.game.get(square);
            if (piece && piece.color === this.game.turn()) {
                this.selectedSquare = square;
                const moves = this.game.moves({ square: square, verbose: true });
                this.possibleMoves = moves.map(m => m.to);
                this.renderBoard();
            }
        } else {
            const movingPiece = this.game.get(this.selectedSquare);
            const isPawn = movingPiece && movingPiece.type === 'p';
            const isPromotionRank = (square.endsWith('8') && movingPiece.color === 'w') ||
                (square.endsWith('1') && movingPiece.color === 'b');

            // Check if this move is a legal pawn promotion
            if (isPawn && isPromotionRank) {
                const legalMoves = this.game.moves({ square: this.selectedSquare, verbose: true });
                const isLegalTarget = legalMoves.some(m => m.to === square);

                if (isLegalTarget) {
                    // Save state and prompt user for promotion piece
                    this.pendingPromotion = { from: this.selectedSquare, to: square };
                    this.showPromotionDialog(movingPiece.color);
                    return;
                }
            }

            // Standard move execution
            const move = this.game.move({
                from: this.selectedSquare,
                to: square,
                promotion: 'q'
            });

            this.selectedSquare = null;
            this.possibleMoves = [];

            if (move) {
                this.finalizeMove();
            } else {
                const piece = this.game.get(square);
                if (piece && piece.color === this.game.turn()) {
                    this.selectedSquare = square;
                    const moves = this.game.moves({ square: square, verbose: true });
                    this.possibleMoves = moves.map(m => m.to);
                }
                this.renderBoard();
            }
        }
    },

    showPromotionDialog: function (color) {
        const existing = document.getElementById("promotion-dialog");
        if (existing) existing.remove();

        const dialog = document.createElement("div");
        dialog.id = "promotion-dialog";
        dialog.className = "promotion-dialog-overlay";

        const content = document.createElement("div");
        content.className = "promotion-dialog-content";

        const pieces = ['q', 'r', 'b', 'n'];
        pieces.forEach(pType => {
            const btn = document.createElement("button");
            btn.className = "promotion-piece-btn";
            const symbolObj = { type: pType, color: color };
            btn.innerText = this.getPieceSymbol(symbolObj);
            btn.addEventListener("click", () => {
                this.completePromotion(pType);
                dialog.remove();
            });
            content.appendChild(btn);
        });

        dialog.appendChild(content);
        document.body.appendChild(dialog);
    },

    completePromotion: function (promoPiece) {
        if (!this.pendingPromotion) return;

        const move = this.game.move({
            from: this.pendingPromotion.from,
            to: this.pendingPromotion.to,
            promotion: promoPiece
        });

        this.selectedSquare = null;
        this.possibleMoves = [];
        this.pendingPromotion = null;

        if (move) {
            this.finalizeMove();
        } else {
            this.renderBoard();
        }
    },

    finalizeMove: function () {
        this.renderBoard();
        const fen = this.game.fen();
        const isGameOver = typeof this.game.isGameOver === 'function' ? this.game.isGameOver() : this.game.game_over();

        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnMoveCompleted', fen, isGameOver);
        }
    },

    makeAiMove: function () {
        const moves = this.game.moves();
        if (moves.length === 0) return;

        const randomMove = moves[Math.floor(Math.random() * moves.length)];
        this.game.move(randomMove);
        this.renderBoard();

        const fen = this.game.fen();
        const isGameOver = typeof this.game.isGameOver === 'function' ? this.game.isGameOver() : this.game.game_over();
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnMoveCompleted', fen, isGameOver);
        }
    },

    getPieceSymbol: function (piece) {
        const symbols = {
            p: '♟', r: '♜', n: '♞', b: '♝', q: '♛', k: '♚',
            P: '♙', R: '♖', N: '♘', B: '♗', Q: '♕', K: '♔'
        };
        return symbols[piece.color === 'w' ? piece.type.toUpperCase() : piece.type.toLowerCase()] || '';
    }
};

window.initializeChessGame = function (dotNetObj, config) {
    window.chessManager.init(dotNetObj, config);
};

window.loadChessFen = function (fen) {
    window.chessManager.loadFen(fen);
};

window.makeAiMove = function () {
    window.chessManager.makeAiMove();
};