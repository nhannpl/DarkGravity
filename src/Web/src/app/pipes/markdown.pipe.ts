import { Pipe, PipeTransform } from '@angular/core';
import { marked } from 'marked';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
    name: 'markdown',
    standalone: true
})
export class MarkdownPipe implements PipeTransform {
    constructor(private sanitizer: DomSanitizer) { }

    transform(value: string | undefined): SafeHtml {
        if (!value) return '';
        // marked.parse is synchronous if options.async is not set to true
        const html = marked.parse(value) as string;
        return this.sanitizer.bypassSecurityTrustHtml(html);
    }
}
