import {useEffect, useState} from "react";
import bffFetch from "../../../services/bffFetch.tsx";


const SAS_CACHE_KEY = "avatar_sas_cache";

function getSasCache(): Record<string, { url: string; expiresAt: number }> {
    try {
        const raw = localStorage.getItem(SAS_CACHE_KEY);
        return raw ? JSON.parse(raw) : {};
    } catch {
        return {};
    }
}

function setSasCache(cache: Record<string, { url: string; expiresAt: number }>) {
    try {
        localStorage.setItem(SAS_CACHE_KEY, JSON.stringify(cache));
    } catch (e) {
        console.warn("Failed to save SAS cache", e);
    }
}

interface AvatarProps{
    PhotoUrl?: string | null,
    TextIfPhotoNull: string,
    IsDirect?: boolean;
}

export function AvatarFromSource({PhotoUrl, TextIfPhotoNull, IsDirect} : AvatarProps){
    const [photoSrc, setPhotoSrc] = useState<string | null>(null);

    useEffect(() => {
        if (!PhotoUrl) {
            setPhotoSrc(null);
            return;
        }

        if (IsDirect) {
            setPhotoSrc(PhotoUrl);
            return;
        }

        const cache = getSasCache();
        const cached = cache[PhotoUrl];
        const now = Date.now();

        if (cached && cached.expiresAt > now) {
            console.log(`LOAD FROM CACHE ${PhotoUrl} ${cached.url}`)
            setPhotoSrc(cached.url);
            return;
        }
        
        console.log(`LOAD NEW ${PhotoUrl} ${cached.url}`)
        
        bffFetch(PhotoUrl)
            .then(res => res.json())
            .then(data => {
                const url = data.url;
                const expiresAt = now + data.expireTimeMillis;
                
                const newCache = { ...cache, [PhotoUrl]: { url, expiresAt } };
                setSasCache(newCache);
                
                setPhotoSrc(data.url)
            })
            .catch(() => setPhotoSrc(null));
        
        
    }, [PhotoUrl, IsDirect]);
    
    return (
        <div className="avatar">
            {photoSrc ? (
                <img src={photoSrc} alt={TextIfPhotoNull} onError={() => setPhotoSrc(null)} />
            ) : (
                <div className="avatar-placeholder">{TextIfPhotoNull|| '?'}</div>
            )}
        </div>
    );
}