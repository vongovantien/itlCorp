import { Component, ViewChild, ElementRef, Input, EventEmitter, Output, ChangeDetectionStrategy } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { Crystal } from '../../models/report/crystal.model';
import { PopupBase } from 'src/app/popup.base';
import { environment } from 'src/environments/environment';
import { ConfirmPopupComponent } from '../popup';
import { InjectViewContainerRefDirective } from '../../directives/inject-view-container-ref.directive';

@Component({
    selector: 'app-report-preview',
    templateUrl: './report-preview.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportPreviewComponent extends PopupBase {

    @Input() modalId: any;
    @Input() data: Crystal = null;
    @ViewChild('formReport', { static: true }) frm: ElementRef;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;
    @Output() $invisible: EventEmitter<any> = new EventEmitter<any>();
    @Output() onConfirmEdoc: EventEmitter<any> = new EventEmitter<any>();

    constructor(private sanitizer: DomSanitizer,
    ) {
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
        // return this.sanitizer.bypassSecurityTrustResourceUrl('http://localhost:53717');
        return this.sanitizer.bypassSecurityTrustResourceUrl(`${environment.HOST.REPORT}`);
    }

    onHide() {
        this.$invisible.emit();
    }

    onConfirmSaveEdoc() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Confirm save eDOC',
            body: 'Are you sure to save this file',
        }, () => {
            this.onConfirmEdoc.emit();
        });
    }
}
