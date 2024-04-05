import { useMutation, useQuery} from "@tanstack/react-query";
import axios from "axios";
import { LobbyInfo } from "../../models/Lobby";
import { useUser } from "../../userHooks";
import { getQueryClient } from "../../services/queryClient";

export const HomeKeys = {
  lobbies: ["lobbies"],
};

const queryClient = getQueryClient();

export const useGetLobbiesQuery = () => {
  return useQuery({
    queryKey: HomeKeys.lobbies,
    queryFn: async (): Promise<LobbyInfo[]> => {
      const url = "/api/lobby";
      const response = await axios.get(url);
      return response.data;
    },
  });
};

export const useCreateLobbyMutation = () => {
  const user = useUser();

  return useMutation({
    mutationFn: async (): Promise<string> => {
      const url = "/api/lobby";
      const response = await axios.post(url, {
        username: user?.preferred_username,
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: HomeKeys.lobbies
      });
    },
  });
};
