import { Injectable, NgZone } from '@angular/core';
import { Subject } from 'rxjs';

export interface UploadProgress {
  FileId: string;
  FileName: string;
  Progress: number;
  Status: string;
}

@Injectable({
  providedIn: 'root',
})
export class UploadService {
  private ws!: WebSocket;
  private progressSubject = new Subject<UploadProgress>();
  progress$ = this.progressSubject.asObservable();

  constructor(private ngZone: NgZone) {
    this.connect();
  }

  private connect() {
    this.ws = new WebSocket('ws://localhost:5259/ws/upload');

    this.ws.onmessage = (event) => {
      const data = JSON.parse(event.data);

      // console.log("Data : ", data)

      this.ngZone.run(() => {
        this.progressSubject.next(data);
      });
    };
  }
}