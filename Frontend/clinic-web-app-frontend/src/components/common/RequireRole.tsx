import {useAuth} from "../../services/states/userState.tsx";
import React from "react";

export const Roles = {
    Patient: 'client',
    Doctor: 'doctor',
    Receptionist: 'receptionist',
} as const; 

export type Role = typeof Roles[keyof typeof Roles];

interface RequireRoleProps{
    roles?: Role[];
    children?: React.ReactNode;
    fallback?: React.ReactNode;
}
export function RequireRole({roles=[], children, fallback}: RequireRoleProps){
    const context = useAuth()
    if(context.state.status !== "authorized")
    {
        return (<></>);
    }
    
    const currentRoles = context.state.data.getRoles();
    const containsAnyRole=  roles.reduce(
        (accumulator, currentValue) => accumulator || (currentRoles.find(x=>x == currentValue) != null),
        false
    )
    
    if(!containsAnyRole && roles?.length > 0)
    {
        return (<>{fallback}</>);
    }
    
    return (
        <>
            {children}
        </>
    );
}