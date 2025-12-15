import { useState } from "react";
import { useQuery, useMutation } from "@tanstack/react-query";
import { askAIQuestion, fetchHelpTopics } from "../api/aiHelp";
import { SparklesIcon, LightBulbIcon } from "@heroicons/react/24/outline";
import { useAuth } from "../state/AuthContext";

const AIHelp = () => {
  const { isAuthed } = useAuth();
  const [question, setQuestion] = useState("");
  const [chatHistory, setChatHistory] = useState<Array<{ question: string; answer: string; suggestedActions: string[] }>>([]);

  const { data: topics } = useQuery({
    queryKey: ["help-topics"],
    queryFn: fetchHelpTopics,
    enabled: isAuthed,
  });

  const askMutation = useMutation({
    mutationFn: askAIQuestion,
    onSuccess: (response) => {
      setChatHistory((prev) => [
        ...prev,
        {
          question,
          answer: response.answer,
          suggestedActions: response.suggestedActions,
        },
      ]);
      setQuestion("");
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (question.trim()) {
      askMutation.mutate(question);
    }
  };

  const handleTopicClick = (topic: string) => {
    setQuestion(`Tell me about ${topic.toLowerCase()}`);
  };

  if (!isAuthed) {
    return <p className="text-center text-rose-600">Please log in to access AI help.</p>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-purple-500 to-pink-600 text-white shadow-lg">
          <SparklesIcon className="h-6 w-6" />
        </div>
        <div>
          <h1 className="text-3xl font-bold text-slate-900">AI Assistant</h1>
          <p className="mt-1 text-slate-600">Get help understanding the Community Finance system</p>
        </div>
      </div>

      {/* Help Topics */}
      {topics && topics.length > 0 && (
        <div className="card">
          <h2 className="mb-4 text-lg font-semibold text-slate-900">Quick Help Topics</h2>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {topics.map((topic, index) => (
              <button
                key={index}
                onClick={() => handleTopicClick(topic.topic)}
                className="flex items-start gap-3 rounded-lg border border-slate-200 bg-white p-4 text-left transition-all hover:border-blue-300 hover:bg-blue-50 hover:shadow-md"
              >
                <span className="text-2xl">{topic.icon}</span>
                <div>
                  <p className="font-semibold text-slate-900">{topic.topic}</p>
                  <p className="mt-1 text-xs text-slate-600">{topic.description}</p>
                </div>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Chat Interface */}
      <div className="card">
        <div className="mb-4 flex items-center gap-2">
          <LightBulbIcon className="h-5 w-5 text-blue-600" />
          <h2 className="text-lg font-semibold text-slate-900">Ask a Question</h2>
        </div>

        <form onSubmit={handleSubmit} className="mb-6">
          <div className="flex gap-2">
            <input
              type="text"
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              placeholder="Ask me anything about the system..."
              className="flex-1 input-field"
              disabled={askMutation.isPending}
            />
            <button
              type="submit"
              disabled={askMutation.isPending || !question.trim()}
              className="btn-primary"
            >
              {askMutation.isPending ? "Thinking..." : "Ask"}
            </button>
          </div>
        </form>

        {/* Chat History */}
        {chatHistory.length > 0 && (
          <div className="space-y-4 max-h-96 overflow-y-auto">
            {chatHistory.map((chat, index) => (
              <div key={index} className="space-y-2">
                <div className="rounded-lg bg-blue-50 p-3">
                  <p className="text-sm font-semibold text-blue-900">You asked:</p>
                  <p className="text-sm text-blue-800">{chat.question}</p>
                </div>
                <div className="rounded-lg bg-slate-50 p-3">
                  <p className="text-sm font-semibold text-slate-900">AI Assistant:</p>
                  <p className="mt-1 whitespace-pre-line text-sm text-slate-700">{chat.answer}</p>
                  {chat.suggestedActions.length > 0 && (
                    <div className="mt-3">
                      <p className="text-xs font-semibold text-slate-600">Suggested Actions:</p>
                      <ul className="mt-1 list-disc list-inside space-y-1">
                        {chat.suggestedActions.map((action, actionIndex) => (
                          <li key={actionIndex} className="text-xs text-slate-600">{action}</li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        {chatHistory.length === 0 && !askMutation.isPending && (
          <div className="rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 p-8 text-center">
            <SparklesIcon className="mx-auto h-12 w-12 text-slate-400" />
            <p className="mt-4 text-sm font-medium text-slate-600">
              Ask me anything about the Community Finance system!
            </p>
            <p className="mt-2 text-xs text-slate-500">
              Try asking about contributions, loans, goals, or any other feature.
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default AIHelp;


