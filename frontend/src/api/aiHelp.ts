import api from "./client";

export interface AIHelpRequest {
  question: string;
}

export interface AIHelpResponse {
  answer: string;
  suggestedActions: string[];
}

export interface HelpTopic {
  topic: string;
  description: string;
  icon: string;
}

export const askAIQuestion = async (question: string): Promise<AIHelpResponse> => {
  const { data } = await api.post<AIHelpResponse>("/api/aihelp/ask", { question });
  return data;
};

export const fetchHelpTopics = async (): Promise<HelpTopic[]> => {
  const { data } = await api.get<HelpTopic[]>("/api/aihelp/topics");
  return data;
};


