import { groupBy } from "../../../../utilities/arrayUtils.ts";
import { type CategoryDto, servicesApi } from "../../../../services/api/ServicesApi.ts";
import { useEffect, useState } from "react";
import type { ServiceDto } from "./ServicesPage.tsx";
import "./ServicesPage.css";
import { Link } from "react-router-dom";

interface ServiceViewProps {
    service: ServiceDto;
}

export function ServiceView({ service }: ServiceViewProps) {
    return (
        <Link to={`view-services?id=${service.id}`} className="service-link">
            <div className={`service-card ${service.isActive ? "active" : "inactive"}`}>
                <div className="service-info">
                    <div className="service-name">{service.serviceName}</div>
                    <span className={`service-status ${service.isActive ? "active" : "inactive"}`}>
                        {service.isActive ? "Active" : "Inactive"}
                    </span>
                </div>
                <div className="service-price">${service.price}</div>
            </div>
        </Link>
    );
}

interface GroupedBySpecializationServicesProps {
    category: CategoryDto;
}

export function GroupedBySpecializationServices({ category }: GroupedBySpecializationServicesProps) {
    const [services, setServices] = useState<Array<ServiceDto>>();
    const [expandedGroups, setExpandedGroups] = useState<Record<string, boolean>>({});

    useEffect(() => {
        const loadData = async () => {
            try {
                const result = await servicesApi.getServices(category.id);
                if (result.type === "ok") {
                    setServices(result.value.services);
                } else {
                    setServices([]);
                }
            } catch (err) {
                console.log(err);
            }
        };
        loadData();
    }, [category.id]);

    if (!services) {
        return null;
    }

    const toggleGroup = (name: string) => {
        setExpandedGroups(prev => ({ ...prev, [name]: !prev[name] }));
    };

    const groupedServices = groupBy(services, i => i.specializationName);

    return (
        <div className="groups-container">
            {Object.entries(groupedServices).map(([specializationName, items]) => {
                const isExpanded = expandedGroups[specializationName] || false;

                return (
                    <div key={specializationName} className="specialization-group">
                        <div
                            className={`specialization-header ${isExpanded ? "expanded" : ""}`}
                            onClick={() => toggleGroup(specializationName)}
                        >
                            <span className="arrow">{isExpanded ? "▼" : "▶"}</span>
                            <span className="specialization-title">{specializationName}</span>
                            <span className="specialization-count">({items.length})</span>
                        </div>
                        {isExpanded && (
                            <div className="services-list">
                                {items.map((service) => (
                                    <ServiceView key={service.id} service={service} />
                                ))}
                            </div>
                        )}
                    </div>
                );
            })}
        </div>
    );
}