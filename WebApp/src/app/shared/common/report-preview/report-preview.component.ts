import { Component, ViewChild, ElementRef, Input } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { API_MENU } from 'src/constants/api-menu.const';
import { Crystal } from '../../models/report/crystal.model';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'app-report-preview',
    templateUrl: './report-preview.component.html'
})
export class ReportPreviewComponent extends PopupBase {

    @Input() modalId: any;
    @Input() data: Crystal = null;
    @ViewChild('formReport', { static: true }) frm: ElementRef;

    constructor(private sanitizer: DomSanitizer,
        private api_menu: API_MENU) {
        super();
    }

    ngOnInit() {
    }

    ngAfterViewInit(): void {
        if (this.data != null && this.frm) {
            this.frm.nativeElement.submit();
        }
    }

    submitForm(event) {
        return true;
    }
    get value() {
        if (this.data != null && this.frm) {
            return JSON.stringify(this.data);
        }
    }
    get scr() {
        // http://localhost:51830/Default.aspx
        return this.sanitizer.bypassSecurityTrustResourceUrl(this.api_menu.Report);
    }
}
