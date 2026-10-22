var astar = {
    init: function(grid) {
        for(var x = 0, xl = grid.length; x < xl; x++) {
            for(var y = 0, yl = grid[x].length; y < yl; y++) {
                var node = grid[x][y];
                node.f = 0; node.g = 0; node.h = 0;
                node.cost = node.type;
                node.visited = false; node.closed = false; node.parent = null;
            }
        }
    },
    heap: function() { return new BinaryHeap(function(node) { return node.f; }); },
    search: function(grid, start, end, diagonal, heuristic) {
        astar.init(grid);
        heuristic = heuristic || astar.manhattan;
        diagonal = !!diagonal;
        var openHeap = astar.heap();
        openHeap.push(start);
        while(openHeap.size() > 0) {
            var currentNode = openHeap.pop();
            if(currentNode === end) {
                var curr = currentNode; var ret = [];
                while(curr.parent) { ret.push(curr); curr = curr.parent; }
                return ret.reverse();
            }
            currentNode.closed = true;
            var neighbors = astar.neighbors(grid, currentNode, diagonal);
            for(var i=0, il = neighbors.length; i < il; i++) {
                var neighbor = neighbors[i];
                if(neighbor.closed || neighbor.isWall()) continue;
                var gScore = currentNode.g + neighbor.cost;
                var beenVisited = neighbor.visited;
                if(!beenVisited || gScore < neighbor.g) {
                    neighbor.visited = true; neighbor.parent = currentNode;
                    neighbor.h = neighbor.h || heuristic(neighbor.pos, end.pos);
                    neighbor.g = gScore; neighbor.f = neighbor.g + neighbor.h;
                    if (!beenVisited) openHeap.push(neighbor);
                    else openHeap.rescoreElement(neighbor);
                }
            }
        }
        return [];
    },
    manhattan: function(pos0, pos1) {
        return Math.abs (pos1.x - pos0.x) + Math.abs (pos1.y - pos0.y);
    },
    neighbors: function(grid, node, diagonals) {
        var ret = []; var x = node.x; var y = node.y;
        if(grid[x-1] && grid[x-1][y]) ret.push(grid[x-1][y]);
        if(grid[x+1] && grid[x+1][y]) ret.push(grid[x+1][y]);
        if(grid[x] && grid[x][y-1]) ret.push(grid[x][y-1]);
        if(grid[x] && grid[x][y+1]) ret.push(grid[x][y+1]);
        return ret;
    }
};