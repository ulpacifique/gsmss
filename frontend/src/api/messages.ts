import api from "./client";

export interface Message {
  messageId: number;
  senderId: number;
  senderName: string;
  senderProfilePicture?: string;
  receiverId: number;
  receiverName: string;
  receiverProfilePicture?: string;
  content: string;
  isRead: boolean;
  createdAt: string;
}

export interface Conversation {
  otherUserId: number;
  otherUserName: string;
  otherUserEmail: string;
  otherUserProfilePicture?: string;
  lastMessage: Message;
  unreadCount: number;
}

export const fetchConversations = async (): Promise<Conversation[]> => {
  const { data } = await api.get<Conversation[]>("/api/messages/conversations");
  return data;
};

export const fetchMessages = async (otherUserId: number): Promise<Message[]> => {
  const { data } = await api.get<Message[]>(`/api/messages/conversation/${otherUserId}`);
  return data;
};

export const sendMessage = async (receiverId: number, content: string): Promise<Message> => {
  const { data } = await api.post<Message>("/api/messages", { receiverId, content });
  return data;
};

export const markConversationAsRead = async (senderId: number, receiverId: number): Promise<void> => {
  await api.put(`/api/messages/mark-read/${senderId}/${receiverId}`);
};

export const getUnreadMessageCount = async (): Promise<number> => {
  const { data } = await api.get<{ count: number }>("/api/messages/unread-count");
  return data.count;
};


