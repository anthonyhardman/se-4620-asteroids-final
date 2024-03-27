import { QueryCache, QueryClient } from "@tanstack/react-query";
import toast, { ErrorIcon } from "react-hot-toast";

const addErrorAsToast = async (error: any) => {
  console.log(error);
  const message = "Error with request. Check the console or logs for details."

  toast(
    (t: any) => (
      <div className="row">
        <div className="col-auto my-auto">
          <ErrorIcon />
        </div>
        <div className="col my-auto"> {message}</div>
        <div className="col-auto my-auto">
          <button
            onClick={() => toast.dismiss(t.id)}
            className="btn btn-outline-secondary py-1 px-2"
          >
            <i className="bi bi-x"></i>
          </button>
        </div>
      </div>
    ),
    {
      duration: Infinity,
    }
  );
};

export function createInfoToast(children: JSX.Element, onClose: () => void = () => { }) {
  const closeHandler = (t: any) => {
    toast.dismiss(t.id);
    onClose();
  }
  toast(
    (t: any) => (
      <div className="row">
        <div className="col-auto my-auto">
          <i className="bi bi-info-circle-fill"></i>
        </div>
        <div className="col my-auto">{children}</div>
        <div className="col-auto my-auto">
          <button
            onClick={() => closeHandler(t)}
            className="btn btn-outline-secondary py-1"
          >
            X
          </button>
        </div>
      </div>
    ),
    {
      duration: Infinity,
      style: {
        maxWidth: "75em",
      },
    }
  )
}

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: addErrorAsToast
  }),
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 0,
    },
    mutations: {
      retry: 0,
    },
  },
});

export const getQueryClient = () => {
  return queryClient;
};
