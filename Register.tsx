import type { FormEvent } from "react";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../state/AuthContext";

const Register = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    phoneNumber: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleChange = (field: keyof typeof form, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await register(form);
      navigate("/");
    } catch (err: any) {
      const errorMessage = err?.response?.data?.message ?? err?.message ?? "Registration failed";
      console.error("Registration error:", err);
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="mx-auto max-w-md space-y-6">
      <div className="text-center">
        <h1 className="text-2xl font-semibold">Create your account</h1>
        <p className="text-sm text-slate-600">
          This will also set your header credentials for authenticated calls.
        </p>
        <p className="mt-2 text-xs text-slate-500">
          💡 Tip: Use an email containing "admin" or ending with "@admin.com" or "@community.com" to create an Admin account. Otherwise, you'll be registered as a Member.
        </p>
      </div>
      <form onSubmit={handleSubmit} className="card space-y-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="text-sm font-medium text-slate-700">
              First name
            </label>
            <input
              required
              value={form.firstName}
            autoComplete="given-name"
              onChange={(e) => handleChange("firstName", e.target.value)}
              className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
            />
          </div>
          <div>
            <label className="text-sm font-medium text-slate-700">
              Last name
            </label>
            <input
              required
              value={form.lastName}
            autoComplete="family-name"
              onChange={(e) => handleChange("lastName", e.target.value)}
              className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
            />
          </div>
        </div>
        <div>
          <label className="text-sm font-medium text-slate-700">Email</label>
          <input
            type="email"
            required
            value={form.email}
            autoComplete="email"
            onChange={(e) => handleChange("email", e.target.value)}
            className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-slate-700">Password</label>
          <input
            type="password"
            required
            value={form.password}
            autoComplete="new-password"
            onChange={(e) => handleChange("password", e.target.value)}
            className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-slate-700">
            Phone number (optional)
          </label>
          <input
            value={form.phoneNumber}
            autoComplete="tel"
            onChange={(e) => handleChange("phoneNumber", e.target.value)}
            className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
          />
        </div>
        {error && (
          <div className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">
            {error}
          </div>
        )}
        <button
          type="submit"
          disabled={loading}
          className="w-full rounded-md bg-slate-900 px-3 py-2 text-white hover:bg-slate-800 disabled:opacity-60"
        >
          {loading ? "Creating account..." : "Create account"}
        </button>
      </form>
      <p className="text-center text-sm text-slate-600">
        Already have an account?{" "}
        <Link to="/login" className="font-medium text-blue-600">
          Sign in
        </Link>
      </p>
    </div>
  );
};

export default Register;

