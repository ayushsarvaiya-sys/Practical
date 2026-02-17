import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { UploadService, UploadProgress } from '../Services/upload-service';

@Component({
  selector: 'app-upload',
   standalone: true,
  imports: [CommonModule],
  templateUrl: './new.html',
  styleUrl: './new.css',
})
export class New  implements OnInit{
  uploads: UploadProgress[] = [];

  constructor(private uploadService: UploadService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.uploadService.progress$.subscribe(data => {
      const index = this.uploads.findIndex(x => x.FileId === data.FileId);

      if(index > -1)
      {
        this.uploads[index] = {...data};
      }
      else
      {
        this.uploads.push(data);
      }

      // this.uploads.forEach(u => {
      //   console.log("Data 1: ", u);
      // })

      console.log("Data 1: ", this.uploads);
      
      this.cdr.detectChanges();
    });
  }
}
