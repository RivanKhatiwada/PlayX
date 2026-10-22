window.barricadeWrapper = {
    dotNetRef: null,
    config: null,

    init: function (dotNetObj, config) {
        this.dotNetRef = dotNetObj;
        this.config = config;

        // Ensure Daniel's hidden elements exist
        if (!document.getElementById("walls")) {
            const taContainer = document.createElement("div");
            taContainer.style.display = "none";
            taContainer.innerHTML = `
                <textarea id='nowalls'>0-1,1-2,2-3,3-4,4-5,5-6,6-7,7-8,0-9,1-10,2-11,3-12,4-13,5-14,6-15,7-16,9-10,10-11,11-12,12-13,13-14,14-15,15-16,16-17,9-18,10-19,11-20,12-21,13-22,14-23,15-24,16-25,18-19,19-20,20-21,21-22,22-23,23-24,24-25,25-26,18-27,19-28,20-29,21-30,22-31,23-32,24-33,25-34,27-28,28-29,29-30,30-31,31-32,32-33,33-34,34-35,27-36,28-37,29-38,30-39,31-40,32-41,33-42,34-43,36-37,37-38,38-39,39-40,40-41,41-42,42-43,43-44,36-45,37-46,38-47,39-48,40-49,41-50,42-51,43-52,45-46,46-47,47-48,48-49,49-50,50-51,51-52,52-53,45-54,46-55,47-56,48-57,49-58,50-59,51-60,52-61,54-55,55-56,56-57,57-58,58-59,59-60,60-61,61-62,54-63,55-64,56-65,57-66,58-67,59-68,60-69,61-70,63-64,64-65,65-66,66-67,67-68,68-69,69-70,70-71,63-72,64-73,65-74,66-75,67-76,68-77,69-78,70-79</textarea>
                <textarea id='walls'></textarea>
                <textarea id='possibleWalls'></textarea>
                <div id='showCompPieces'><span>Walls: </span><span class='wallVal'>10</span></div>
                <div id='showPlayerPieces'><span>Walls: </span><span class='wallVal'>10</span></div>
            `;
            document.body.appendChild(taContainer);
        }

        // Target the Blazor component container
        const container = document.getElementById("barricade-board-container");
        if (container) {
            // If the #board element isn't already inside the container, move/render it there
            let board = document.getElementById("board");
            if (!board) {
                // Construct a basic fallback table structure if Daniel's static html isn't parsed
                board = document.createElement("table");
                board.id = "board";
                // Build default row 1 with computer and row 9 with player
                // (Alternatively, ensure your index.html or wrapper injects the full table markup)
            }
            container.innerHTML = "";
            if (board) container.appendChild(board);
        }

        this.setupBindings();
    },

    setupBindings: function () {
        $(document).off('click', '.piecePlayer, .pieceComputer').on('click', '.piecePlayer, .pieceComputer', function() {
            var pieceClass = $(this).attr('class');
            var isComp = pieceClass.includes('pieceComputer');

            if (window.barricadeWrapper.config.isSolo && isComp) {
                alert("Please don't touch the computer.");
                return;
            }

            if (window.barricadeWrapper.config.isOnline) {
                var hostTurn = window.barricadeWrapper.config.isHost;
                if (hostTurn && isComp) return;
                if (!hostTurn && !isComp) return;
            }

            piece_moves = possibleMoves($(this).parent().attr('data-pos'), false, isComp ? $('.piecePlayer').parent().attr('data-pos') :$('.pieceComputer').parent().attr('data-pos'), null);
        });

        $(document).off('click', '.movableGreenSpaces').on('click', '.movableGreenSpaces', function() {
            movePiece(piece_moves[0], $(this).attr('data-pos'));
            window.barricadeWrapper.notifyStateChange();
        });
    },

    notifyStateChange: function () {
        const state = {
            playerLoc: $('.piecePlayer').parent().attr('data-pos'),
            computerLoc: $('.pieceComputer').parent().attr('data-pos'),
            walls: $('#walls').val(),
            nowalls: $('#nowalls').val(),
            playerWalls: $('#showPlayerPieces').children().next().text(),
            compWalls: $('#showCompPieces').children().next().text()
        };

        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnStateChanged', JSON.stringify(state));
        }

        if (this.config.isSolo) {
            setTimeout(() => {
                computerMove();
                window.barricadeWrapper.notifyStateChange();
            }, 400);
        }
    },

    loadStateJson: function (jsonStr) {
        const state = JSON.parse(jsonStr);
        $('.piecePlayer').parent().children().remove();$('.pieceComputer').parent().children().remove();

        $(`td[data-pos='${state.playerLoc}']`).append("<div class='piecePlayer'></div>");
        $(`td[data-pos='${state.computerLoc}']`).append("<div class='pieceComputer'></div>");

        $('#walls').val(state.walls);
        $('#nowalls').val(state.nowalls);
        $('#showPlayerPieces').children().next().text(state.playerWalls);
        $('#showCompPieces').children().next().text(state.compWalls);
    }
};

window.initializeBarricadeGame = (ref, cfg) => window.barricadeWrapper.init(ref, cfg);
window.loadBarricadeState = (json) => window.barricadeWrapper.loadStateJson(json);