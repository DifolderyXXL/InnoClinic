import React from "react";
import {NavLink} from "react-router";
import "./RouteLinkCard.css";

interface RouteLinkCardProps{
    to: string;
    children?: React.ReactNode;
}

export function RouteLinkCard({to, children}: RouteLinkCardProps) {
    return (
        <NavLink to={to} className="routeLink">{children}</NavLink>
    );
}