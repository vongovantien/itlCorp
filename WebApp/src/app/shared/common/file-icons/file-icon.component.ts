import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'icon-file',
    templateUrl: './file-icon.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppIconFileComponent implements OnInit {
    @Input() url: string = '';
    constructor() { }

    ngOnInit(): void { }
}
