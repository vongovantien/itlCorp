import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, Input } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { API_MENU } from 'src/constants/api-menu.const';
import { Crystal } from '../../models/report/crystal.model';

@Component({
  selector: 'app-report-preview',
  templateUrl: './report-preview.component.html',
  styleUrls: ['./report-preview.component.scss']
})
export class ReportPreviewComponent implements OnInit, AfterViewInit {
  @Input() data: Crystal;
  @ViewChild('formReport') frm: ElementRef;
  
  ngAfterViewInit(): void {
    if(this.data != null && this.frm){
      this.frm.nativeElement.submit();
    }
  }

  constructor(private sanitizer: DomSanitizer,
    private api_menu: API_MENU) { }

  ngOnInit() {
  }
  submitForm(event){
    return true;
  }
  get value(){
    if(this.data != null && this.frm){
      return JSON.stringify(this.data);
    }
  }
  get scr(){
    return this.sanitizer.bypassSecurityTrustResourceUrl("http://localhost:51830/Default.aspx");
    }
}
