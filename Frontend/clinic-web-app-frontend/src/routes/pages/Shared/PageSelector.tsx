import React, { useState, useEffect } from "react";
import "./PageSelector.css";

interface PageSelectorProps {
    currentPage: number;
    total: number;
    pageSize?: number;
    onPageChange: (page: number) => void;
}

export function PageSelector({ currentPage, total, pageSize = 50, onPageChange }: PageSelectorProps) {
    const totalPages = Math.max(1, Math.ceil(total / pageSize));

    const goToPage = (page: number) => {
        if (page < 1 || page > totalPages) return;
        onPageChange(page);
    };

    return (
        <div className="page-selector">
            <button
                className="pager-btn"
                onClick={() => goToPage(currentPage - 1)}
                disabled={currentPage === 1}
            >
                Prev
            </button>

            <span className="pager-info">
                Page <strong>{currentPage}</strong> of <strong>{totalPages}</strong>
            </span>

            <button
                className="pager-btn"
                onClick={() => goToPage(currentPage + 1)}
                disabled={currentPage === totalPages}
            >
                Next
            </button>
        </div>
    );
}

interface DiscretePageSelectorProps<T> {
    tabs: Array<T>;
    start?: T;
    onPageChange: (tab: T) => void;
    getId: (tab: T) => string | number;
    children: (activeTab: T) => React.ReactNode;
}

export function DiscretePageSelector<T>({
                                            tabs,
                                            onPageChange,
                                            children,
                                            start,
                                            getId
                                        }: DiscretePageSelectorProps<T>) {
    if (!tabs || tabs.length === 0) return null;

    const [currentPageId, setCurrentPageId] = useState<string | number | null>(
        start ? getId(start) : getId(tabs[0])
    );

    useEffect(() => {
        if (start) {
            setCurrentPageId(getId(start));
        }
    }, [start, getId]);

    const goToPage = (tab: T) => {
        const id = getId(tab);
        setCurrentPageId(id);
        onPageChange(tab);
    };

    return (
        <div className="discrete-tabs-container">
            {tabs.map((tab) => {
                const id = getId(tab);
                const isActive = currentPageId === id;

                return (
                    <button
                        key={id}
                        className={`discreteTabButton ${isActive ? 'active' : ''}`}
                        onClick={() => goToPage(tab)}
                    >
                        {children(tab)}
                    </button>
                );
            })}
        </div>
    );
}