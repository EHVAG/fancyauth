using System;
using System.Threading.Tasks;

namespace Fancyauth.Wrapped
{
    public static class FixIce
    {
        public static Task FromAsyncVoid(Func<Ice.AsyncCallback, object, Ice.AsyncResult> begin, Action<Ice.AsyncResult> end)
        {
            var tcs = new TaskCompletionSource<object>();
            begin(GenericAsyncVoidCallback, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task FromAsyncVoid<TP1>(TP1 p1, Func<TP1, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Action<Ice.AsyncResult> end)
        {
            var tcs = new TaskCompletionSource<object>();
            begin(p1, GenericAsyncVoidCallback, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task FromAsyncVoid<TP1, TP2>(TP1 p1, TP2 p2, Func<TP1, TP2, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Action<Ice.AsyncResult> end)
        {
            var tcs = new TaskCompletionSource<object>();
            begin(p1, p2, GenericAsyncVoidCallback, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task FromAsyncVoid<TP1, TP2, TP3>(TP1 p1, TP2 p2, TP3 p3, Func<TP1, TP2, TP3, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Action<Ice.AsyncResult> end)
        {
            var tcs = new TaskCompletionSource<object>();
            begin(p1, p2, p3, GenericAsyncVoidCallback, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task FromAsyncVoid<TP1, TP2, TP3, TP4, TP5>(TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5,
            Func<TP1, TP2, TP3, TP4, TP5, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Action<Ice.AsyncResult> end)
        {
            var tcs = new TaskCompletionSource<object>();
            begin(p1, p2, p3, p4, p5, GenericAsyncVoidCallback, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        private static void GenericAsyncVoidCallback(Ice.AsyncResult iar)
        {
            var state = (Tuple<TaskCompletionSource<object>, Action<Ice.AsyncResult>>)iar.AsyncState;
            var tcs = state.Item1;
            var end = state.Item2;
            try {
                end(iar);
                tcs.TrySetResult(null);
            } catch (Exception ex) {
                tcs.TrySetException(ex);
            }
        }

        public static Task<TRet> FromAsync<TRet>(Func<Ice.AsyncCallback, object, Ice.AsyncResult> begin, Func<Ice.AsyncResult, TRet> end)
        {
            var tcs = new TaskCompletionSource<TRet>();
            begin(GenericAsyncCallback<TRet>, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task<TRet> FromAsync<TRet, TP1>(TP1 p1, Func<TP1, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Func<Ice.AsyncResult, TRet> end)
        {
            var tcs = new TaskCompletionSource<TRet>();
            begin(p1, GenericAsyncCallback<TRet>, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task<TRet> FromAsync<TRet, TP1, TP2>(TP1 p1, TP2 p2, Func<TP1, TP2, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Func<Ice.AsyncResult, TRet> end)
        {
            var tcs = new TaskCompletionSource<TRet>();
            begin(p1, p2, GenericAsyncCallback<TRet>, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task<TRet> FromAsync<TRet, TP1, TP2, TP3>(TP1 p1, TP2 p2, TP3 p3, Func<TP1, TP2, TP3, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Func<Ice.AsyncResult, TRet> end)
        {
            var tcs = new TaskCompletionSource<TRet>();
            begin(p1, p2, p3, GenericAsyncCallback<TRet>, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        public static Task<TRet> FromAsync<TRet, TP1, TP2, TP3, TP4, TP5>(TP1 p1, TP2 p2, TP3 p3, TP4 p4, TP5 p5,
            Func<TP1, TP2, TP3, TP4, TP5, Ice.AsyncCallback, object, Ice.AsyncResult> begin, Func<Ice.AsyncResult, TRet> end)
        {
            var tcs = new TaskCompletionSource<TRet>();
            begin(p1, p2, p3, p4, p5, GenericAsyncCallback<TRet>, Tuple.Create(tcs, end));
            return tcs.Task;
        }

        private static void GenericAsyncCallback<T>(Ice.AsyncResult iar)
        {
            var state = (Tuple<TaskCompletionSource<T>, Func<Ice.AsyncResult, T>>)iar.AsyncState;
            var tcs = state.Item1;
            var end = state.Item2;
            try {
                tcs.TrySetResult(end(iar));
            } catch (Exception ex) {
                tcs.TrySetException(ex);
            }
        }
    }
}

