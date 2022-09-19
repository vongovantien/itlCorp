import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Clipboard } from '@angular/cdk/clipboard';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: '[app-copy-to-clipboard]',
    template: `
        <ng-container *ngIf = "hasIcon; else textOnly">
             <span NoDbClick (click)="onClicked()">
                <i class="{{icon}} {{class}}"></i>
            </span>
            &nbsp; <span>{{text}}</span>
        </ng-container>
        
        <ng-template #textOnly>
            <span NoDbClick (click)="onClicked()"> 
                {{text}}
            </span>
        </ng-template>
    
    `

})
export class CopyToClipboardComponent implements OnInit {
    @Input() set notification(alert: string) { this._notification = alert; }
    @Input() set text(t: string) { this._text = t; }
    @Input() set icon(i: string) { this._icon = i; }
    @Input() set hasIcon(h: boolean) { this._hasIcon = h; }
    @Input() set class(c: string) { this._class = c; }
    @Output() onCopy: EventEmitter<any> = new EventEmitter<any>();


    get text() { return this._text; }
    get icon() { return this._icon; }
    get notification() { return this._notification }
    get hasIcon() { return this._hasIcon; }
    get class() { return this._class; }

    private message: string = ' was copied';
    private _text: string = "";
    private _icon: string = "la la-copy";
    private _notification: string;
    private _hasIcon: boolean = false;
    private _class: string = 'text-success';


    constructor(
        private clipboard: Clipboard,
        private _toastService: ToastrService,
    ) {
        this.clipboard.copy(this._text);
    }

    ngOnInit(): void {
    }

    onClicked() {
        this.onCopy.emit(this._text);
        this.clipboard.copy(this._text);
        this._toastService.success(this._notification !== undefined ? this._notification : this._text + this.message);

    }
}
