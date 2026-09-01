import { createBrowserRouter, Navigate } from "react-router-dom";
import { AuthLayout } from "@/layouts/AuthLayout";
import { AppLayout } from "@/layouts/AppLayout";
import { ProtectedRoute } from "@/components/shared/ProtectedRoute";

import { LoginPage } from "@/pages/auth/LoginPage";
import { RegisterPage } from "@/pages/auth/RegisterPage";
import { HomePage } from "@/pages/HomePage";
import { MyGroupsPage } from "@/pages/teacher/MyGroupsPage";
import { LandingPage } from "@/pages/public/LandingPage";
import { UsersPage } from "@/pages/admin/UsersPage";
import { GroupsPage } from "@/pages/crm/GroupsPage";
import { GroupProfilePage } from "@/pages/crm/GroupProfilePage";
import { FinancePage } from "@/pages/crm/FinancePage";
import { LeadsPage } from "@/pages/crm/LeadsPage";
import { StudentsPage } from "@/pages/crm/StudentsPage";
import { StudentProfilePage } from "@/pages/crm/StudentProfilePage";
import { TasksPage } from "@/pages/crm/TasksPage";
import { BranchesPage } from "@/pages/admin/BranchesPage";
import { RoomsPage } from "@/pages/admin/RoomsPage";
import { StaffPage } from "@/pages/admin/StaffPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <LandingPage />,
  },
  {
    element: <AuthLayout />,
    children: [
      { path: "/login", element: <LoginPage /> },
      { path: "/register", element: <RegisterPage /> },
    ],
  },
  {
    element: (
      <ProtectedRoute>
        <AppLayout />
      </ProtectedRoute>
    ),
    children: [
      { path: "/home", element: <HomePage /> },
      {
        path: "/crm/leads",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <LeadsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/crm/students",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <StudentsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/crm/students/:id",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <StudentProfilePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/crm/groups",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <GroupsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/crm/groups/:id",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <GroupProfilePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/crm/finance",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <FinancePage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/crm/tasks",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <TasksPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/teacher/my-groups",
        element: (
          <ProtectedRoute roles={["Teacher", "OrgAdmin", "SuperAdmin", "Owner"]}>
            <MyGroupsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/admin/branches",
        element: (
          <ProtectedRoute roles={["OrgAdmin", "SuperAdmin", "Owner"]}>
            <BranchesPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/admin/rooms",
        element: (
          <ProtectedRoute roles={["OrgAdmin", "SuperAdmin", "Owner"]}>
            <RoomsPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/admin/staff",
        element: (
          <ProtectedRoute roles={["Owner", "SuperAdmin", "OrgAdmin", "Staff"]}>
            <StaffPage />
          </ProtectedRoute>
        ),
      },
      {
        path: "/admin/users",
        element: (
          <ProtectedRoute roles={["Owner"]}>
            <UsersPage />
          </ProtectedRoute>
        ),
      },
    ],
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
