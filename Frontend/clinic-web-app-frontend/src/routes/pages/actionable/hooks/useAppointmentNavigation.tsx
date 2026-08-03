import {useCallback} from "react";
import {useNavigate} from "react-router";

interface UseAppointmentNavigationOptions {
    officeId?: number | string;
    specId?: number | string;
    serviceId?: number | string;
    doctorId?: number | string;
}

export const useAppointmentNavigation = () => {
    const navigate = useNavigate();

    const navigateToAppointment  = useCallback((options: UseAppointmentNavigationOptions = {}) => {
        const cleanParams: Record<string, string> = {};

        Object.entries(options).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                cleanParams[key] = String(value);
            }
        });
        const queryString = new URLSearchParams(cleanParams).toString();

        const targetUrl = queryString
            ? `/make-appointment?${queryString}`
            : '/make-appointment';

        navigate(targetUrl);
    }, []);

    return [navigateToAppointment ] as const;
};