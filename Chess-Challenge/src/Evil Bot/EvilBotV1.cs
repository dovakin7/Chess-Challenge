using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class EvilBotV1 : IChessBot
{
    bool White;
    Timer t;
    public Move Think(Board board, Timer timer)
    {
        White = board.IsWhiteToMove;
        t = timer;
        Move res;
        if (White)
            res = max_fct(board, int.MinValue, int.MaxValue, 0).Key;
        else
            res = min_fct(board, int.MinValue, int.MaxValue, 0).Key;

        if (res.IsNull)
        {
            dynamic allMoves = board.GetLegalMoves();
            Random rng = new();
            return allMoves[rng.Next(allMoves.Length)];
        }

        return res;
    }

    KeyValuePair<Move, int> min_fct(Board board, int alpha, int beta, int depth)
    {
        depth++;
        if (EndBranchSearch(board, depth))
        {
            return new KeyValuePair<Move, int>(default, GetBoardEval(board));
        }
        Move m = default;
        int u = int.MaxValue;

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            dynamic res = max_fct(board, alpha, beta, depth);
            board.UndoMove(move);

            if (res.Value < u)
            {
                u = res.Value;
                m = move;
            }
            if (u <= alpha)
                break;
            beta = Math.Max(beta, u);
        }
        return new KeyValuePair<Move, int>(m, u);
    }

    KeyValuePair<Move, int> max_fct(Board board, int alpha, int beta, int depth)
    {
        depth++;
        if (EndBranchSearch(board, depth))
        {
            return new KeyValuePair<Move, int>(default, GetBoardEval(board));
        }
        // int u = int.MinValue;
        Move m = default;
        int u = int.MinValue;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            dynamic res = min_fct(board, alpha, beta, depth);
            board.UndoMove(move);

            if (res.Value > u)
            {
                u = res.Value;
                m = move;
            }
            if (u >= beta)
                break;

            alpha = Math.Max(alpha, u);
        }
        return new KeyValuePair<Move, int>(m, u); ;
    }

    int GetBoardEval(Board board)
    {
        int eval = 0;
        if (board.IsInCheckmate())
        {
            if (board.SquareIsAttackedByOpponent(board.GetKingSquare(true)))
                return int.MinValue;
            return int.MaxValue;
        }
        if (board.IsDraw())
            return 0;

        foreach (PieceList p in board.GetAllPieceLists())
        {
            eval += GetPieceEvalFromPieceList(p) * p.Count * (p.IsWhitePieceList ? 1 : -1);
        }

        eval += GetPieceEvalFromActivity(board);

        return eval;
    }

    bool EndBranchSearch(Board board, int curDepth)
    {
        return board.IsDraw() || board.IsInCheckmate() || curDepth > 4;
    }

    int GetPieceEvalFromPieceList(PieceList p)
    {
        switch (p.TypeOfPieceInList)
        {
            case PieceType.Pawn:
                return 100;
            case PieceType.Knight:
                return 300;
            case PieceType.Bishop:
                return 300;
            case PieceType.Rook:
                return 500;
            case PieceType.Queen:
                return 1000;
            default: return 0;
        }
    }
    
    int GetPieceEvalFromActivity(Board board)
    {
        int eval = board.GetLegalMoves().Length * (board.IsWhiteToMove ? 1 : -1);
        if (board.TrySkipTurn())
        {
            eval += board.GetLegalMoves().Length * (board.IsWhiteToMove ? 1 : -1);
            board.UndoSkipTurn();
        }
        return eval;
    }

}