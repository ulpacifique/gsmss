import { useState, useEffect, useRef } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "../api/client";
import { useAuth } from "../state/AuthContext";
import { UserPlusIcon, MagnifyingGlassIcon } from "@heroicons/react/24/outline";

interface Message {
  messageId: number;
  senderId: number;
  senderName: string;
  receiverId: number;
  receiverName: string;
  content: string;
  isRead: boolean;
  createdAt: string;
}

interface Conversation {
  otherUserId: number;
  otherUserName: string;
  otherUserEmail?: string;
  otherUserProfilePicture?: string;
  lastMessage?: Message;
  unreadCount: number;
}

interface User {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  profilePictureUrl?: string;
  role: string;
}

const Chat = () => {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [messageText, setMessageText] = useState("");
  const [showUserList, setShowUserList] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const { data: conversations } = useQuery({
    queryKey: ["conversations"],
    queryFn: async () => {
      const { data } = await api.get<Conversation[]>("/api/messages/conversations");
      return data;
    },
    staleTime: 10000, // Consider data fresh for 10 seconds
    refetchInterval: 15000, // Refresh every 15 seconds (reduced from 5)
  });

  // Fetch all users for starting new conversations
  const { data: allUsers } = useQuery({
    queryKey: ["all-users"],
    queryFn: async () => {
      try {
        const { data } = await api.get<User[]>("/api/admin/users");
        // Filter out current user
        return data.filter((u) => u.userId !== user?.userId);
      } catch (error) {
        // If admin endpoint fails, try members endpoint
        try {
          const { data } = await api.get<User[]>("/api/admin/users/members");
          return data.filter((u) => u.userId !== user?.userId);
        } catch {
          return [];
        }
      }
    },
    enabled: !!user,
  });

  const { data: messages } = useQuery({
    queryKey: ["messages", selectedUserId],
    queryFn: async () => {
      if (!selectedUserId) return [];
      const { data } = await api.get<Message[]>(`/api/messages/conversation/${selectedUserId}`);
      return data;
    },
    enabled: selectedUserId !== null,
    staleTime: 5000, // Consider data fresh for 5 seconds
    refetchInterval: 10000, // Refresh every 10 seconds when viewing conversation (reduced from 3)
  });

  const sendMessageMutation = useMutation({
    mutationFn: async (content: string) => {
      const { data } = await api.post("/api/messages", {
        receiverId: selectedUserId,
        content,
        messageType: "chat",
      });
      return data;
    },
    onSuccess: () => {
      setMessageText("");
      queryClient.invalidateQueries({ queryKey: ["messages", selectedUserId] });
      queryClient.invalidateQueries({ queryKey: ["conversations"] });
    },
  });

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const handleSend = () => {
    if (!messageText.trim() || !selectedUserId) return;
    sendMessageMutation.mutate(messageText.trim());
  };

  const selectedUser = conversations?.find((c) => c.otherUserId === selectedUserId);

  // Filter users based on search query
  const filteredUsers = allUsers?.filter((u) => {
    const fullName = `${u.firstName} ${u.lastName}`.toLowerCase();
    const email = u.email.toLowerCase();
    const query = searchQuery.toLowerCase();
    return fullName.includes(query) || email.includes(query);
  }) || [];

  // Get users who don't have conversations yet
  const usersWithoutConversations = filteredUsers.filter(
    (u) => !conversations?.some((c) => c.otherUserId === u.userId)
  );

  const handleStartConversation = (userId: number) => {
    setSelectedUserId(userId);
    setShowUserList(false);
    setSearchQuery("");
  };

  return (
    <div className="flex h-[calc(100vh-200px)] gap-4">
      {/* Conversations List */}
      <div className="w-80 rounded-xl bg-white shadow-lg ring-1 ring-slate-100 flex flex-col">
        <div className="border-b border-slate-200 p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-bold text-slate-900">Messages</h2>
            <button
              onClick={() => setShowUserList(!showUserList)}
              className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-blue-700 transition-colors"
              title="Start new conversation"
            >
              <UserPlusIcon className="h-4 w-4" />
              New
            </button>
          </div>
          {showUserList && (
            <div className="mb-3">
              <div className="relative">
                <MagnifyingGlassIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search users..."
                  className="w-full rounded-lg border border-slate-300 bg-slate-50 pl-9 pr-3 py-2 text-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>
            </div>
          )}
        </div>
        <div className="flex-1 overflow-y-auto" style={{ maxHeight: "calc(100vh - 300px)" }}>
          {showUserList ? (
            <div className="divide-y divide-slate-100">
              {usersWithoutConversations.length > 0 ? (
                usersWithoutConversations.map((u) => (
                  <button
                    key={u.userId}
                    onClick={() => handleStartConversation(u.userId)}
                    className="w-full p-4 text-left transition-colors hover:bg-slate-50"
                  >
                    <div className="flex items-center gap-3">
                      {u.profilePictureUrl ? (
                        <img
                          src={u.profilePictureUrl}
                          alt={`${u.firstName} ${u.lastName}`}
                          className="h-12 w-12 rounded-full object-cover"
                        />
                      ) : (
                        <div className="flex h-12 w-12 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-indigo-500 text-white font-bold">
                          {u.firstName[0]?.toUpperCase()}
                        </div>
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="font-semibold text-slate-900 truncate">
                          {u.firstName} {u.lastName}
                        </p>
                        <p className="mt-1 truncate text-xs text-slate-500">
                          {u.email}
                        </p>
                        <p className="mt-1 text-xs text-blue-600">
                          {u.role === "Admin" ? "Administrator" : "Member"}
                        </p>
                      </div>
                    </div>
                  </button>
                ))
              ) : (
                <div className="p-8 text-center text-slate-500">
                  {searchQuery ? "No users found" : "No users available"}
                </div>
              )}
            </div>
          ) : (
            <>
              {conversations && conversations.length > 0 ? (
                <div className="divide-y divide-slate-100">
                  {conversations.map((conv) => (
                    <button
                      key={conv.otherUserId}
                      onClick={() => setSelectedUserId(conv.otherUserId)}
                      className={`w-full p-4 text-left transition-colors hover:bg-slate-50 ${
                        selectedUserId === conv.otherUserId ? "bg-blue-50" : ""
                      }`}
                    >
                      <div className="flex items-center gap-3">
                        {conv.otherUserProfilePicture ? (
                          <img
                            src={conv.otherUserProfilePicture}
                            alt={conv.otherUserName}
                            className="h-12 w-12 rounded-full object-cover"
                          />
                        ) : (
                          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-indigo-500 text-white font-bold">
                            {conv.otherUserName[0]?.toUpperCase()}
                          </div>
                        )}
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center justify-between">
                            <p className="font-semibold text-slate-900 truncate">
                              {conv.otherUserName}
                            </p>
                            {conv.unreadCount > 0 && (
                              <span className="flex h-5 w-5 items-center justify-center rounded-full bg-gradient-to-r from-blue-600 to-indigo-600 text-xs font-bold text-white shadow-md">
                                {conv.unreadCount}
                              </span>
                            )}
                          </div>
                          {conv.lastMessage && (
                            <p className="mt-1 truncate text-sm text-slate-600">
                              {conv.lastMessage.content}
                            </p>
                          )}
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              ) : (
                <div className="p-8 text-center text-slate-500">
                  <p className="mb-2">No conversations yet</p>
                  <button
                    onClick={() => setShowUserList(true)}
                    className="text-sm text-blue-600 hover:text-blue-700 font-medium"
                  >
                    Start a new conversation
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>

      {/* Chat Area */}
      <div className="flex-1 rounded-xl bg-white shadow-lg ring-1 ring-slate-100 flex flex-col">
        {selectedUserId ? (
          <>
            <div className="border-b border-slate-200 p-4">
              <div className="flex items-center gap-3">
                {selectedUser?.otherUserProfilePicture ? (
                  <img
                    src={selectedUser.otherUserProfilePicture}
                    alt={selectedUser.otherUserName}
                    className="h-10 w-10 rounded-full object-cover"
                  />
                ) : (
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-indigo-500 text-white font-bold">
                    {selectedUser?.otherUserName[0]?.toUpperCase()}
                  </div>
                )}
                <div>
                  <h3 className="font-semibold text-slate-900">
                    {selectedUser?.otherUserName}
                  </h3>
                  <p className="text-xs text-slate-500">{selectedUser?.otherUserEmail}</p>
                </div>
              </div>
            </div>

            <div className="flex-1 overflow-y-auto p-4 space-y-4">
              {messages && messages.length > 0 ? (
                messages.map((msg) => {
                  const isOwn = msg.senderId === user?.userId;
                  return (
                    <div
                      key={msg.messageId}
                      className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
                    >
                      <div
                        className={`max-w-[70%] rounded-lg px-4 py-2 ${
                          isOwn
                            ? "bg-blue-600 text-white"
                            : "bg-slate-100 text-slate-900"
                        }`}
                      >
                        {!isOwn && (
                          <p className="mb-1 text-xs font-semibold opacity-75">
                            {msg.senderName}
                          </p>
                        )}
                        <p className="text-sm">{msg.content}</p>
                        <p className={`mt-1 text-xs ${isOwn ? "text-blue-100" : "text-slate-500"}`}>
                          {new Date(msg.createdAt).toLocaleTimeString()}
                        </p>
                      </div>
                    </div>
                  );
                })
              ) : (
                <div className="text-center text-slate-500">No messages yet</div>
              )}
              <div ref={messagesEndRef} />
            </div>

            <div className="border-t border-slate-200 p-4">
              <div className="flex gap-2">
                <input
                  type="text"
                  value={messageText}
                  onChange={(e) => setMessageText(e.target.value)}
                  onKeyPress={(e) => e.key === "Enter" && handleSend()}
                  placeholder="Type a message..."
                  className="flex-1 rounded-lg border border-slate-300 px-4 py-2 focus:border-blue-500 focus:ring-blue-500"
                />
                <button
                  onClick={handleSend}
                  disabled={!messageText.trim() || sendMessageMutation.isPending}
                  className="btn-primary"
                >
                  Send
                </button>
              </div>
            </div>
          </>
        ) : (
          <div className="flex h-full items-center justify-center text-slate-500">
            Select a conversation to start chatting
          </div>
        )}
      </div>
    </div>
  );
};

export default Chat;

