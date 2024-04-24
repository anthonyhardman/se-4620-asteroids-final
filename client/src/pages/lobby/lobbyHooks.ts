import { useMutation, useQuery } from "@tanstack/react-query";
import { useUser } from "../../userHooks";
import axios from "axios";
import { getQueryClient } from "../../services/queryClient";
import { HomeKeys } from "../home/homeHooks";
import { LobbyInfo } from "../../models/Lobby";

const queryClient = getQueryClient();

export const LobbyKeys = {
  lobby: (id?: string) => ["lobby", id] as const,
};

export const useGetLobbyInfoQuery = (id?: string) =>
  useQuery({
    queryKey: LobbyKeys.lobby(id),
    queryFn: async (): Promise<LobbyInfo | undefined> => {
      if (!id) return undefined;
      const url = `/api/lobby/${id}`;
      const response = await axios.get(url);
      return response.data;
    },
  });

export const useStartGameMutation = () => {
  const user = useUser();
  return useMutation({
    mutationFn: async ({ lobbyId, maxAsteroids }: { lobbyId: string, maxAsteroids: number }) => {
      const url = "/api/lobby";
      const response = await axios.put(url, {
        username: user?.preferred_username,
        lobbyId,
        maxAsteroids,
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: HomeKeys.lobbies });
    },
  });
};

export const useKillLobbyMutation = () => {
  return useMutation({
    mutationFn: async (lobbyId: string) => {
      const url = `/api/lobby/kill/${lobbyId}`;
      const response = await axios.put(url);
      return response.data;
    }
  });
};

export const useUpdatePlayerColorMutation = (lobbyId?: string) => {
  const user = useUser();
  return useMutation({
    mutationFn: async (color: string) => {
      const url = `/api/lobby/color`;
      const response = await axios.put(url, {
        username: user?.preferred_username,
        lobbyId,
        color
      })
      return response.data;
    }
  })
}
