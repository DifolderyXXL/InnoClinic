import React from "react";
import { NavLink } from "react-router-dom";
import "./RouteLinkCard.css";

interface RouteLinkCardProps {
    to: string;
    children?: React.ReactNode;
}

export function RouteLinkCard({ to, children }: RouteLinkCardProps) {
    return (
        <NavLink
            to={to}
            className={({ isActive }) => `routeLink ${isActive ? "active" : ""}`}
        >
            {children}
        </NavLink>
    );
}