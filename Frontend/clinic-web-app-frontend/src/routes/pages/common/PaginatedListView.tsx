import { useState, useEffect } from "react";
import {PageSelector} from "../Shared/PageSelector.tsx";

export interface PaginatedResult<T> {
    items: T[];
    total: number;
    error?: string;
}

interface PaginatedListViewProps<T> {
    currentPage: number;
    pageSize: number;
    onPageChange: (page: number) => void;

    fetchRequest: (page: number) => Promise<PaginatedResult<T>>;

    dependencies: any[];

    renderItems: (items: T[]) => React.ReactNode;
}

export function PaginatedListView<T>({
                                         currentPage,
                                         pageSize,
                                         onPageChange,
                                         fetchRequest,
                                         dependencies,
                                         renderItems
                                     }: PaginatedListViewProps<T>) {
    const [items, setItems] = useState<T[]>([]);
    const [total, setTotal] = useState<number>(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let isMounted = true;
        setLoading(true);
        setError(null);

        fetchRequest(currentPage)
            .then(result => {
                if (!isMounted) return;

                if (result.error) {
                    setError(result.error);
                } else {
                    setItems(result.items);
                    setTotal(result.total);
                }
            })
            .catch(() => {
                if (isMounted) setError("Unhandled error occurred");
            })
            .finally(() => {
                if (isMounted) setLoading(false);
            });

        return () => { isMounted = false; };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentPage, ...dependencies]);

    if (loading) {
        return <div style={{ textAlign: 'center', padding: '40px' }}>Loading...</div>;
    }

    if (error) {
        return <div style={{ textAlign: 'center', padding: '40px', color: 'red' }}>{error}</div>;
    }

    return (
        <>
            {renderItems(items)}

            <PageSelector
                pageSize={pageSize}
                total={total}
                onPageChange={onPageChange}
            />
        </>
    );
}