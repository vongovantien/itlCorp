import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-copy-to-clipboard',
    template:
    `
    <ng-container>
        <span NoDbClick (click)="onClicked($event)">
            <i class="la la-{{icon}} text-success"></i>
        </span>
    </ng-container>
    `,
})
export class CopyToClipboardComponent implements OnInit {
    @Input() set notification(alert: string) { this._notification = alert; }
    @Input() set text(t: string) { this._text = t; }
    @Input() set icon(i: string) { this._icon = i; }

    get text() { return this._text; }
    get icon() { return this._icon; }
    get notification() { return this._notification}
    
    
    private message: string = ' was copied';
    private _text: string = "";
    private _icon: string = 'la la-copy';
    private _notification: string;


    constructor(
        private clipboard: Clipboard,
        private _toastService: ToastrService,
    ){
    }

    ngOnInit(): void {
    }

    onClicked() {
        this.clipboard.copy(this._text);
        this._toastService.success(this._notification !== undefined ? this._notification: this._text + this.message);
    }
}
