import { useMutation } from "@tanstack/react-query";
import { useUser } from "../../userHooks"
import axios from "axios";
import { getQueryClient } from "../../services/queryClient";
import { HomeKeys } from "../home/homeHooks";

const queryClient = getQueryClient();

export const LobbyKeys = {

}

export const useStartGameMutation = () => {
  const user = useUser();
  return useMutation({
    mutationFn: async (lobbyId: string) => {
      const url = "/api/lobby";
      const response = await axios.put(url, {
        username: user?.preferred_username, lobbyId
      })
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: HomeKeys.lobbies});
    }
  })
}