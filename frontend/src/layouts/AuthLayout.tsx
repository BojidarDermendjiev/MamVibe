import { Link, Outlet } from "react-router-dom";

export default function AuthLayout() {
  return (
    <div className="min-h-screen bg-peach flex items-center justify-center p-4">
      <div className="w-full max-w-md animate-fade-in">
        <div className="text-center mb-8">
          <Link to="/" className="inline-flex items-center gap-2">
            <img
              src="/logo.png"
              alt="MamVibe"
              className="h-24 w-24 object-contain"
            />
            <span className="text-2xl font-bold text-primary">MamVibe</span>
          </Link>
        </div>
        <div className="bg-white rounded-2xl shadow-md border border-lavender/30 p-8 transition-shadow duration-300 hover:shadow-lg">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
