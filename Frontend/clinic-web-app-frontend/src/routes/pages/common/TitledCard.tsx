import React from "react";
import "./TitledCard.css"

interface  TitledCard{
    title?: string;
    children?: React.ReactNode
}
export function TitledCard({title, children}: TitledCard){
    return (
        <div className="card-block">
            {title && <span className="card-title">
                {title}
            </span>}
            <div className="card-content">
                {children}
            </div>
        </div>
    );
}