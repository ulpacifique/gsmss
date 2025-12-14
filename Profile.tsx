import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { fetchProfile, updateProfile } from "../api/members";
import { useState, useEffect } from "react";
import { useAuth } from "../state/AuthContext";

const Profile = () => {
  const qc = useQueryClient();
  const { user, isAuthed } = useAuth();
  const { data: profile } = useQuery({
    queryKey: ["profile"],
    queryFn: fetchProfile,
    enabled: isAuthed,
    retry: (failureCount, error: any) => {
      if (error?.response?.status === 401) return false;
      return failureCount < 3;
    },
  });

  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    phoneNumber: "",
    email: "",
    profilePictureUrl: "",
  });

  const [profilePictureFile, setProfilePictureFile] = useState<File | null>(null);
  const [profilePicturePreview, setProfilePicturePreview] = useState<string | null>(null);

  useEffect(() => {
    if (profile) {
      setForm({
        firstName: profile.firstName,
        lastName: profile.lastName,
        phoneNumber: profile.phoneNumber ?? "",
        email: profile.email,
        profilePictureUrl: profile.profilePictureUrl ?? "",
      });
      if (profile.profilePictureUrl) {
        setProfilePicturePreview(profile.profilePictureUrl);
      }
    }
  }, [profile]);

  const handlePictureChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setProfilePictureFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setProfilePicturePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const mutation = useMutation({
    mutationFn: async (data: typeof form) => {
      // If there's a profile picture file, convert it to base64 or upload it
      let profilePictureUrl = form.profilePictureUrl;
      if (profilePictureFile) {
        // Convert to base64 for now (in production, you'd upload to a file server)
        // Limit file size to 2MB
        if (profilePictureFile.size > 2 * 1024 * 1024) {
          throw new Error("Profile picture must be less than 2MB");
        }
        const base64 = await new Promise<string>((resolve, reject) => {
          const reader = new FileReader();
          reader.onload = () => resolve(reader.result as string);
          reader.onerror = reject;
          reader.readAsDataURL(profilePictureFile);
        });
        profilePictureUrl = base64;
      }
      return updateProfile({ ...data, profilePictureUrl });
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["profile"] });
      qc.invalidateQueries({ queryKey: ["admin-users"] });
      setProfilePictureFile(null);
      alert("Profile updated successfully! Refresh the page to see your new profile picture in the header.");
    },
    onError: (error: any) => {
      console.error("Profile update error:", error);
    },
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Profile</h1>
        <p className="text-sm text-slate-600">
          Update your member details. Auth headers are attached automatically.
        </p>
      </div>

      <div className="card space-y-4">
        {/* Profile Picture */}
        <div className="flex items-center gap-4">
          <div className="flex-shrink-0">
            {profilePicturePreview ? (
              <img
                src={profilePicturePreview}
                alt="Profile"
                className="h-24 w-24 rounded-full object-cover border-2 border-slate-200"
              />
            ) : (
              <div className="h-24 w-24 rounded-full bg-slate-200 flex items-center justify-center text-slate-400 text-2xl font-semibold">
                {form.firstName?.[0]?.toUpperCase() || form.email?.[0]?.toUpperCase() || "?"}
              </div>
            )}
          </div>
          <div>
            <label className="text-sm font-medium text-slate-700 block mb-2">
              Profile Picture
            </label>
            <input
              type="file"
              accept="image/*"
              onChange={handlePictureChange}
              className="text-sm text-slate-600 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
            />
            <p className="text-xs text-slate-500 mt-1">
              Upload a picture to help others identify you
            </p>
          </div>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label className="text-sm font-medium text-slate-700">
              First name
            </label>
            <input
              value={form.firstName}
              onChange={(e) =>
                setForm((f) => ({ ...f, firstName: e.target.value }))
              }
              className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
            />
          </div>
          <div>
            <label className="text-sm font-medium text-slate-700">
              Last name
            </label>
            <input
              value={form.lastName}
              onChange={(e) =>
                setForm((f) => ({ ...f, lastName: e.target.value }))
              }
              className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
            />
          </div>
        </div>
        <div>
          <label className="text-sm font-medium text-slate-700">
            Email
          </label>
          <input
            type="email"
            value={form.email}
            onChange={(e) =>
              setForm((f) => ({ ...f, email: e.target.value }))
            }
            className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2 bg-slate-50"
            placeholder="your.email@example.com"
          />
          <p className="text-xs text-slate-500 mt-1">
            You can update your email address if needed
          </p>
        </div>
        <div>
          <label className="text-sm font-medium text-slate-700">
            Phone number
          </label>
          <input
            value={form.phoneNumber}
            onChange={(e) =>
              setForm((f) => ({ ...f, phoneNumber: e.target.value }))
            }
            className="mt-1 w-full rounded-md border border-slate-200 px-3 py-2"
            placeholder="123-456-7890"
          />
        </div>
        <button
          onClick={() => mutation.mutate(form)}
          className="rounded-md bg-slate-900 px-3 py-2 text-white hover:bg-slate-800 disabled:opacity-60"
          disabled={mutation.isPending}
        >
          {mutation.isPending ? "Saving..." : "Save changes"}
        </button>
        {mutation.isError && (
          <div className="rounded-md bg-rose-50 px-3 py-2 text-sm text-rose-700">
            Failed to update: {(mutation.error as any)?.response?.data?.message ?? (mutation.error as any)?.message ?? "An error occurred"}
          </div>
        )}
      </div>

      <div className="card text-sm text-slate-600">
        <p>
          Signed in as <span className="font-medium">{user?.email}</span>
        </p>
        <p>Role: {user?.role}</p>
      </div>
    </div>
  );
};

export default Profile;

