import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  FolderKanban,
  CheckSquare,
  Clock3,
  LogOut,
  Code2,
  Menu,
  X,
  ChevronDown,
} from 'lucide-react';

function Navbar({ setIsAuthenticated }) {
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);

  const navItems = [
    {
      path: '/',
      icon: LayoutDashboard,
      label: 'Dashboard',
    },
    {
      path: '/projects',
      icon: FolderKanban,
      label: 'Projects',
    },
    {
      path: '/tasks',
      icon: CheckSquare,
      label: 'Tasks',
    },
    {
      path: '/time-tracking',
      icon: Clock3,
      label: 'Time',
    },
  ];

  const isActive = (path) => {
    if (path === '/') {
      return location.pathname === '/';
    }

    return location.pathname.startsWith(path);
  };

  const handleLogout = () => {
    localStorage.removeItem('devos_token');
    setIsAuthenticated(false);
  };

  const closeMobileMenu = () => {
    setMobileOpen(false);
  };

  return (
    <>
      <header className="sticky top-0 left-0 right-0 z-50 h-16 bg-devos-dark/95 backdrop-blur-xl border-b border-devos-border">
        <div className="h-full max-w-[1440px] mx-auto px-4 sm:px-6">
          <div className="h-full flex items-center">

            {/* Logo */}
            <Link
              to="/"
              onClick={closeMobileMenu}
              className="flex items-center gap-3 shrink-0 group"
            >
              <div
                className="
                  w-9 h-9
                  flex items-center justify-center
                  rounded-xl
                  bg-devos-primary
                  text-white
                  shadow-lg shadow-devos-primary/20
                  transition-all duration-200
                  group-hover:scale-105
                  group-hover:shadow-devos-primary/30
                "
              >
                <Code2 className="w-[18px] h-[18px]" />
              </div>

              <div className="hidden sm:block">
                <div className="text-sm font-bold tracking-tight text-white">
                  DevOS
                </div>

                <div className="text-[10px] text-gray-500 mt-0.5">
                  Developer Workspace
                </div>
              </div>
            </Link>

            {/* Desktop Navigation */}
            <nav className="hidden md:flex items-center ml-10 gap-1">
              {navItems.map((item) => {
                const Icon = item.icon;
                const active = isActive(item.path);

                return (
                  <Link
                    key={item.path}
                    to={item.path}
                    className={`
                      relative
                      flex items-center gap-2
                      h-10
                      px-3.5
                      rounded-lg
                      text-sm font-medium
                      transition-all duration-200
                      ${
                        active
                          ? 'bg-devos-surface text-white'
                          : 'text-gray-500 hover:text-gray-200 hover:bg-devos-surface/60'
                      }
                    `}
                  >
                    <Icon
                      className={`
                        w-4 h-4
                        transition-colors
                        ${
                          active
                            ? 'text-devos-primary'
                            : 'text-gray-500'
                        }
                      `}
                    />

                    <span>{item.label}</span>

                    {active && (
                      <span
                        className="
                          absolute
                          left-3 right-3
                          -bottom-[12px]
                          h-[2px]
                          rounded-full
                          bg-devos-primary
                        "
                      />
                    )}
                  </Link>
                );
              })}
            </nav>

            {/* Right section */}
            <div className="ml-auto flex items-center gap-2">

              {/* Workspace status */}
              <div
                className="
                  hidden lg:flex
                  items-center gap-2
                  h-9
                  px-3
                  rounded-lg
                  bg-devos-surface/60
                  border border-devos-border
                "
              >
                <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />

                <span className="text-xs font-medium text-gray-400">
                  Workspace
                </span>
              </div>

              {/* Divider */}
              <div className="hidden sm:block w-px h-6 bg-devos-border mx-1" />

              {/* User menu */}
              <div className="relative">
                <button
                  type="button"
                  onClick={() => setUserMenuOpen(!userMenuOpen)}
                  className="
                    flex items-center gap-2
                    h-9
                    px-2
                    rounded-lg
                    border border-transparent
                    hover:border-devos-border
                    hover:bg-devos-surface
                    transition-all duration-200
                  "
                  aria-label="Open user menu"
                >
                  <div
                    className="
                      w-7 h-7
                      rounded-md
                      bg-devos-primary/15
                      border border-devos-primary/20
                      flex items-center justify-center
                    "
                  >
                    <Code2 className="w-3.5 h-3.5 text-devos-primary" />
                  </div>

                  <span className="hidden sm:block text-sm font-medium text-gray-300">
                    Developer
                  </span>

                  <ChevronDown
                    className={`
                      hidden sm:block
                      w-3.5 h-3.5
                      text-gray-500
                      transition-transform duration-200
                      ${userMenuOpen ? 'rotate-180' : ''}
                    `}
                  />
                </button>

                {userMenuOpen && (
                  <div
                    className="
                      absolute
                      right-0
                      top-12
                      w-48
                      p-1.5
                      rounded-xl
                      bg-devos-surface
                      border border-devos-border
                      shadow-2xl shadow-black/30
                    "
                  >
                    <div className="px-3 py-2.5 mb-1">
                      <p className="text-xs text-gray-500">
                        Signed in as
                      </p>

                      <p className="text-sm font-medium text-gray-200 mt-0.5">
                        Developer
                      </p>
                    </div>

                    <div className="h-px bg-devos-border mb-1" />

                    <button
                      type="button"
                      onClick={handleLogout}
                      className="
                        w-full
                        flex items-center gap-2
                        px-3 py-2
                        rounded-lg
                        text-sm
                        text-gray-400
                        hover:text-red-400
                        hover:bg-red-500/10
                        transition-colors
                      "
                    >
                      <LogOut className="w-4 h-4" />
                      Sign out
                    </button>
                  </div>
                )}
              </div>

              {/* Mobile button */}
              <button
                type="button"
                onClick={() => setMobileOpen(!mobileOpen)}
                className="
                  md:hidden
                  w-9 h-9
                  flex items-center justify-center
                  rounded-lg
                  border border-devos-border
                  bg-devos-surface/50
                  text-gray-400
                  hover:text-white
                  hover:bg-devos-surface
                  transition-all duration-200
                "
                aria-label="Toggle navigation"
              >
                {mobileOpen ? (
                  <X className="w-4 h-4" />
                ) : (
                  <Menu className="w-4 h-4" />
                )}
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Mobile navigation */}
      {mobileOpen && (
        <div
          className="
            fixed
            top-16
            left-0
            right-0
            z-40
            md:hidden
            bg-devos-dark/98
            backdrop-blur-xl
            border-b border-devos-border
            shadow-2xl shadow-black/20
          "
        >
          <nav className="p-3 space-y-1">
            {navItems.map((item) => {
              const Icon = item.icon;
              const active = isActive(item.path);

              return (
                <Link
                  key={item.path}
                  to={item.path}
                  onClick={closeMobileMenu}
                  className={`
                    flex items-center gap-3
                    px-3
                    py-3
                    rounded-lg
                    text-sm font-medium
                    transition-all duration-200
                    ${
                      active
                        ? 'bg-devos-surface text-white'
                        : 'text-gray-500 hover:text-gray-200 hover:bg-devos-surface/60'
                    }
                  `}
                >
                  <Icon
                    className={`
                      w-4 h-4
                      ${
                        active
                          ? 'text-devos-primary'
                          : 'text-gray-500'
                      }
                    `}
                  />

                  <span>{item.label}</span>

                  {active && (
                    <span className="ml-auto w-1.5 h-1.5 rounded-full bg-devos-primary" />
                  )}
                </Link>
              );
            })}

            <div className="h-px bg-devos-border my-2" />

            <button
              type="button"
              onClick={handleLogout}
              className="
                w-full
                flex items-center gap-3
                px-3 py-3
                rounded-lg
                text-sm font-medium
                text-gray-500
                hover:text-red-400
                hover:bg-red-500/10
                transition-colors
              "
            >
              <LogOut className="w-4 h-4" />
              Sign out
            </button>
          </nav>
        </div>
      )}
    </>
  );
}

export default Navbar;